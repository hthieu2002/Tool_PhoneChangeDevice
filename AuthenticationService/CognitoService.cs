using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using POCO.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AuthenticationService
{
    [Obfuscation(Exclude = false)]
    public class CognitoService
    {
        private readonly string poolId, clientId;
        public CognitoService(string poolId, string clientId)
        {
            this.poolId = poolId;
            this.clientId = clientId;
        }
        public string getIdToken(string username, string password)
        {
            try
            {
                AnonymousAWSCredentials cred = new AnonymousAWSCredentials();

                var cognitoProvider = new AmazonCognitoIdentityProviderClient(cred, RegionEndpoint.APSoutheast1);
                CognitoUserPool userPool = new CognitoUserPool(poolId, clientId, cognitoProvider);
                CognitoUser user = new CognitoUser(username, clientId, userPool, cognitoProvider);

                var response = Task.Run(() => user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
                {
                    Password = password
                })).Result;


                //Console.WriteLine("Access Token");
                //Console.WriteLine(response.AuthenticationResult.AccessToken);
                //Console.WriteLine("Id Token");
                //Console.WriteLine(response.AuthenticationResult.IdToken);
                //Console.WriteLine("SessionID");
                //Console.WriteLine(response.SessionID);
                return response.AuthenticationResult.IdToken;
            }
            catch
            {
                return string.Empty;
            }
        }
        public AWSUserModel getUserInfo(string username, string password)
        {
            try
            {
                AnonymousAWSCredentials cred = new AnonymousAWSCredentials();

                var cognitoProvider = new AmazonCognitoIdentityProviderClient(cred, RegionEndpoint.APSoutheast1);

                CognitoUserPool userPool = new CognitoUserPool(poolId, clientId, cognitoProvider);
                CognitoUser user = new CognitoUser(username, clientId, userPool, cognitoProvider);
                var awsUserModel = new AWSUserModel();

                var taskRun = Task.Run(() => user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
                {
                    Password = password
                })).ContinueWith((task) =>
                {
                    awsUserModel.IdToken = task.Result.AuthenticationResult.IdToken;
                    var userAttrs = user.GetUserDetailsAsync().Result.UserAttributes;
                    for (int i = 0; i < userAttrs.Count; i++)
                    {
                        if (userAttrs[i].Name.Contains("telegramBot"))
                        {
                            awsUserModel.Custom_TelegramBot = userAttrs[i].Value;
                        }
                        if (userAttrs[i].Name.Contains("googleDriveFolderId"))
                        {
                            awsUserModel.Custom_GoogleDriveFolderId = userAttrs[i].Value;
                        }
                        if (userAttrs[i].Name.Contains("telegramRoom"))
                        {
                            awsUserModel.Custom_TelegramRoom = userAttrs[i].Value;
                        }
                        if (userAttrs[i].Name.Contains("isActivated"))
                        {
                            awsUserModel.isActivated = int.Parse(userAttrs[i].Value) == 1;
                        }
                        if (userAttrs[i].Name.Contains("usingKeyStrokes"))
                        {
                            awsUserModel.Custom_UsingKeyStrokes = int.Parse(userAttrs[i].Value);
                        }
                    }
                });
                taskRun.Wait(TimeSpan.FromSeconds(10));
                if (awsUserModel == null
                    || !awsUserModel.isActivated
                    || string.IsNullOrEmpty(awsUserModel.IdToken)
                    || string.IsNullOrEmpty(awsUserModel.Custom_TelegramBot)
                    || string.IsNullOrEmpty(awsUserModel.Custom_TelegramRoom))
                    throw new Exception("NULL Attributes");
                return awsUserModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
