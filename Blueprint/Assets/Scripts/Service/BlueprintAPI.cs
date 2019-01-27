﻿using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using Service.Request;
using Service.Response;

namespace Service {
    public class BlueprintAPI {
        private RestHandler rs;

        // Const
        private const string authenticateEndpoint  = ":8000/api/v1/authenticate";
        private const string registerEndpoint      = ":8000/api/v1/authenticate/register";
        private const string refreshEndpoint       = ":8000/api/v1/authenticate/refresh";
        private const string inventoryEndpoint     = ":8001/api/v1/inventory";
        private const string defaultBaseUrl        = "http://smithwjv.ddns.net";
        
        // Enum
        public enum httpResponseCode {
            ok = 200,
            badRequest = 400,
            unauthorised = 401
        };

        private BlueprintAPI(string baseUrl) {
            this.rs = new RestHandler(baseUrl);
        }

        public static BlueprintAPI WithBaseUrl(string baseUrl) {
            return new BlueprintAPI(baseUrl);
        }
        
        public static BlueprintAPI DefaultCredentials() {
            return new BlueprintAPI(defaultBaseUrl);
        }

        public int RetrieveHTTPCode(WebException e) {
            var responseDetailed = e.Response as HttpWebResponse;
            int httpStatus = (int)responseDetailed.StatusCode;
            return httpStatus;
        }

        private void validateUsernamePassword(string username, string password) {
            // Check validity of password
            if (!rs.CheckPasswordValid(password)) {
                throw new InvalidCredentialException("Password invalid");
            }
            
            // Check validity of username
            if (username.Length >= 16) {
                throw new InvalidCredentialException("Username invalid, must have less than 16 characters");
            }
        }

        private string retrieveErrorJson(WebException e) {
            using (var stream = e.Response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
        
        public async Task<APIResult<UserCredentials, JsonError>> AsyncAuthenticateUser(UserCredentials user) {
            validateUsernamePassword(user.GetUsername(), user.GetPassword());
            
            // Prepare JSON payload & local variables
            string json = JsonUtility.ToJson(new PayloadAuthenticate(user));

            try {
                string response = await rs.PerformAsyncPost(authenticateEndpoint, json);

                ResponseAuthenticate tokens = JsonUtility.FromJson<ResponseAuthenticate>(response);

                // Return APIResult:UserCredentials in success case
                return new APIResult<UserCredentials, JsonError>(
                    new UserCredentials(user.GetUsername(), user.GetPassword(), tokens.access, tokens.refresh)
                );
            }
            catch (WebException e) {
                // Retrieve error payload from WebException
                JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(e));
            
                // Return APIResult:JsonError in error case
                return new APIResult<UserCredentials, JsonError>(error);
            }
        }
        
        public async Task<APIResult<UserCredentials, JsonError>> AsyncRegisterUser(string username, string password) {
            validateUsernamePassword(username, password);
            
            // Prepare JSON payload & local variables
            string json = JsonUtility.ToJson(new PayloadAuthenticate(username, password));

            try {
                string response = await rs.PerformAsyncPost(registerEndpoint, json);

                ResponseAuthenticate tokens = JsonUtility.FromJson<ResponseAuthenticate>(response);

                // Return APIResult:UserCredentials in success case
                return new APIResult<UserCredentials, JsonError>(
                    new UserCredentials(username, password, tokens.access, tokens.refresh)
                );
            } catch (WebException e) {
                // Retrieve error payload from WebException
                JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(e));
            
                // Return APIResult:JsonError in error case
                return new APIResult<UserCredentials, JsonError>(error);
            }
        }

        public async Task<APIResult<ResponseAuthenticate, JsonError>> AsyncRefreshTokens(UserCredentials user) {
            // Prepare JSON payload & local variables
            string json = JsonUtility.ToJson(new RefreshPayload(user.GetRefreshToken()));
            
            try {
                string response = await rs.PerformAsyncPost(refreshEndpoint, json);

                ResponseAuthenticate tokens = JsonUtility.FromJson<ResponseAuthenticate>(response);

                // Return APIResult:ResponseAuthenticate in success case
                return new APIResult<ResponseAuthenticate, JsonError>(tokens);
            } catch (WebException e) {
                // Retrieve error payload from WebException
                JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(e));
                
                // Return APIResult:JsonError in error case
                return new APIResult<ResponseAuthenticate, JsonError>(error);
            }
        }
        
        public async Task<APIResult<ResponseGetInventory, JsonError>> AsyncGetInventory(UserCredentials user) {
            try {
                string response = await rs.PerformAsyncGet(inventoryEndpoint, user.GetAccessToken());

                ResponseGetInventory inventory = JsonUtility.FromJson<ResponseGetInventory>(response);

                // Return APIResult:ResponseGetInventory in success case
                return new APIResult<ResponseGetInventory, JsonError>(inventory);
            } catch (WebException e) when (RetrieveHTTPCode(e) == (int) httpResponseCode.unauthorised) {
                // If access token doesn't match a user, refresh tokens and retry
                APIResult<ResponseAuthenticate, JsonError> refreshedTokens = await AsyncRefreshTokens(user);

                try {
                    string response = await rs.PerformAsyncGet(inventoryEndpoint, refreshedTokens.GetSuccess().access);

                    ResponseGetInventory inventory = JsonUtility.FromJson<ResponseGetInventory>(response);

                    // Return APIResult:ResponseGetInventory in success case
                    return new APIResult<ResponseGetInventory, JsonError>(inventory);
                }
                catch (WebException f) {
                    // Retrieve error payload from WebException
                    JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(f));
                
                    // Return APIResult:JsonError in error case
                    return new APIResult<ResponseGetInventory, JsonError>(error);
                }
            } catch (WebException e) {
                // Retrieve error payload from WebException
                JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(e));
                
                // Return APIResult:JsonError in error case
                return new APIResult<ResponseGetInventory, JsonError>(error);
            }
        }
        
        public async Task<APIResult<Boolean, JsonError>> AsyncAddToInventory(UserCredentials user, ResponseGetInventory items) {
            // Prepare JSON payload
            string json = JsonUtility.ToJson(items);

            try {
                string response = await rs.PerformAsyncPost(inventoryEndpoint, json, user.GetAccessToken());

                // Return APIResult:Boolean in success case
                return new APIResult<Boolean, JsonError>(true);
            } catch (WebException e) when (RetrieveHTTPCode(e) == (int) httpResponseCode.unauthorised) {
                // If access token doesn't match a user, refresh tokens and retry
                APIResult<ResponseAuthenticate, JsonError> refreshedTokens = await AsyncRefreshTokens(user);

                try {
                    string response = await rs.PerformAsyncPost(inventoryEndpoint, json, refreshedTokens.GetSuccess().access);

                    // Return APIResult:Boolean in success case
                    return new APIResult<Boolean, JsonError>(true);
                } catch (WebException f) {
                    // Retrieve error payload from WebException
                    JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(f));
                
                    // Return APIResult:JsonError in error case
                    return new APIResult<Boolean, JsonError>(error);
                }
            } catch (WebException e) {
                // Retrieve error payload from WebException
                JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(e));
                
                // Return APIResult:JsonError in error case
                return new APIResult<Boolean, JsonError>(error);
            }
            
        }
        
        public async Task<APIResult<Boolean, JsonError>> AsyncDeleteInventory(UserCredentials user) {
            try {
                string response = await rs.PerformAsyncDelete(inventoryEndpoint, user.GetAccessToken());

                // Return APIResult:Boolean in success case
                return new APIResult<Boolean, JsonError>(true);
            } catch (WebException e) when (RetrieveHTTPCode(e) == (int) httpResponseCode.unauthorised) {
                // If access token doesn't match a user, refresh tokens and retry
                APIResult<ResponseAuthenticate, JsonError> refreshedTokens = await AsyncRefreshTokens(user);

                try {
                    string response = await rs.PerformAsyncDelete(inventoryEndpoint, refreshedTokens.GetSuccess().access);

                    // Return APIResult:Boolean in success case
                    return new APIResult<Boolean, JsonError>(true);
                } catch (WebException f) {
                    // Retrieve error payload from WebException
                    JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(f));
                
                    // Return APIResult:JsonError in error case
                    return new APIResult<Boolean, JsonError>(error);
                }
            } catch (WebException e) {
                // Retrieve error payload from WebException
                JsonError error = JsonUtility.FromJson<JsonError>(retrieveErrorJson(e));
                
                // Return APIResult:JsonError in error case
                return new APIResult<Boolean, JsonError>(error);
            }
        }

        private class RefreshPayload {
            public string refresh;

            public RefreshPayload(string refresh) {
                this.refresh = refresh;
            }
        }
    }
}