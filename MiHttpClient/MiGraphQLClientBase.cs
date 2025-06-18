using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;
using System.Threading.Tasks;

namespace MiHttpClient
{

    public class MiGraphQLClientBase
    {
        protected readonly GraphQLHttpClient client;
        public MiGraphQLClientBase(string endpoint, string authenticationType, string authenticationValue)
        {
            client = new GraphQLHttpClient(endpoint, new NewtonsoftJsonSerializer());
            //client.HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            //client.HttpClient.DefaultRequestHeaders.Add("authorization", tokenKey);
            client.HttpClient.DefaultRequestHeaders.Add(authenticationType, authenticationValue);
        }

        protected async Task<TResponse> SendQueryAsync<TResponse>(string requestQuery, object requestParam)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = requestQuery,
                    Variables = requestParam
                };

                var response = await client.SendQueryAsync<TResponse>(request);
                return response.Data;
            }
            catch (GraphQLHttpRequestException graphQLException)
            {
                Exception ex = new Exception(graphQLException.Message);
                ex.Data.Add("StatusCode", graphQLException.StatusCode);
                ex.Data.Add("Content", graphQLException.Content);
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
