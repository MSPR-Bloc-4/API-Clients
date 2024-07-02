using System.IO;
using Client_Api.Configuration;
using Client_Api.Repository;
using Client_Api.Repository.Interface;
using Client_Api.Service;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PubSubLibrary;

namespace Client_Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FirebaseConfig>(_configuration.GetSection("FirebaseConfig"));
            var firebaseConfig = _configuration.GetSection("FirebaseConfig").Get<FirebaseConfig>();

            GoogleCredential credential;
            using (var stream = new FileStream(firebaseConfig.ServiceAccountPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream);
            }

            FirestoreDbBuilder builder = new FirestoreDbBuilder
            {
                ProjectId = firebaseConfig.ProjectId,
                DatabaseId = "user",
                Credential = credential
            };

            FirestoreDb db = builder.Build();

            services.AddSingleton(provider =>
            {
                return new FirebaseAuthClient(new FirebaseAuthConfig
                {
                    ApiKey = firebaseConfig.ApiKey,
                    AuthDomain = firebaseConfig.AuthDomain,
                    Providers = new FirebaseAuthProvider[]
                    {
                    new EmailProvider()
                    }
                });
            });
            
            services.AddSingleton<PubSubService>(provider =>
            {
                return new PubSubService(firebaseConfig.ProjectId, credential.ToChannelCredentials());
            });

            services.AddSingleton(db);
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddControllers();
            services.AddSwaggerGen();
            services.AddAuthorization();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });
        }
    }
}
