//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using ModernHttpClient;
//using Newtonsoft.Json;
//using Xamarin.Forms;

//namespace RydeTunes.Network
//{
//    class NetworkInterface
//    {
//        public enum LoginResult
//        {
//            Success,
//            Failure,
//            ResetRequired
//        };

//        public class LoginResponse
//        {
//            [JsonProperty("access_token")]
//            public string AccessToken { get; set; }

//            [JsonProperty("token_type")]
//            public string TokenType { get; set; }

//            [JsonProperty("refresh_token")]
//            public string RefreshToken { get; set; }

//            [JsonProperty("expires_in")]
//            public string ExpiresIn { get; set; }

//            [JsonProperty("scope")]
//            public string Scope { get; set; }

//            [JsonProperty("isPasswordSet")]
//            public bool IsPasswordSet { get; set; }
//        }

//        public const string NetworkErrorTitle = "Network Error";
//        public const string DeviceNetworkConnectivityErrorMessage =
//            "Your device is not currently connected to the network. Please connect and try again.";

//        private static string GoogleAddressApiRoot = "https://maps.googleapis.com/maps/api/";
//        private static string GoogleAddressApiKey = "AIzaSyDOgozNT1BLp_evsHy0P1pfn2Otmem_2p4";
//        private static string LOGIN_ENDPOINT = "oauth/token";

//        private HttpClient _client;
//        private LoginResponse _loginResponse;
//        private bool _hasAttempted;

//        public NetworkInterface()
//        {
//            _client = GetNewHttpClient();
//        }

//        public async void Post(POSTCommand command, bool authorized = true, bool shouldRetry = true)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var endpoint = authorized ? command.UrlWithToken(_loginResponse.AccessToken) : command.EndPoint;
//                var postResponse = await _client.PostAsync(
//                    endpoint,
//                    new StringContent(command.PostBody, Encoding.UTF8, "application/json"));

//                if (!postResponse.IsSuccessStatusCode)
//                {
//                    if (postResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Post(command, authorized);
//                        }, command.OnError);
//                        return;
//                    }
//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        if (shouldRetry)
//                        {
//                            System.Diagnostics.Debug.WriteLine($"Network Error {postResponse.StatusCode} at POST {endpoint}");
//                            var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                            if (retry)
//                            {
//                                command.WasRefreshed = false;
//                                Post(command, authorized);
//                                return;
//                            }
//                        }

//                        command.StatusCode = postResponse.StatusCode;
//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = postResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine(" post {0} response {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(command.OnSuccess);
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Post request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        public async void Post<T>(POSTCommand<T> command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var postResponse = await _client.PostAsync(command.UrlWithToken(_loginResponse.AccessToken),
//                    new StringContent(command.PostBody, Encoding.UTF8, "application/json"));

//                if (!postResponse.IsSuccessStatusCode)
//                {
//                    if (postResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Post<T>(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {postResponse.StatusCode} at POST<T> {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Post(command);
//                            return;
//                        }

//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = postResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine("\n\n url {0}\nresponse {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(
//                        () => command.OnSuccess(JsonConvert.DeserializeObject<T>(responseString)));
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Post request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        public async void Post(POSTCommand<string> command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var postResponse = await _client.PostAsync(command.UrlWithToken(_loginResponse.AccessToken),
//                    new StringContent(command.PostBody, Encoding.UTF8, "application/json"));

//                if (!postResponse.IsSuccessStatusCode)
//                {
//                    if (postResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Post(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {postResponse.StatusCode} at POST<string> {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Post(command);
//                            return;
//                        }

//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = postResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine(" post {0} response {1}", command.EndPoint,
//                        responseString);
//                    var responseObject = JsonConvert.DeserializeObject<POSTResponse>(responseString);
//                    Device.BeginInvokeOnMainThread(() => command.OnSuccess(responseObject.Id));
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Post request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        public async void Put(POSTCommand command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var updateResponse = await _client.PutAsync(command.UrlWithToken(_loginResponse.AccessToken),
//                    new StringContent(command.PostBody, Encoding.UTF8, "application/json"));

//                var responseStatusCode = updateResponse.StatusCode;

//                if (!updateResponse.IsSuccessStatusCode)
//                {
//                    if (responseStatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(onSuccess: () =>
//                        {
//                            command.WasRefreshed = true;
//                            Put(command);
//                        }, onError: () =>
//                        {
//                            command.StatusCode = responseStatusCode;
//                            command.OnError();
//                        });
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {updateResponse.StatusCode} at PUT {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                        "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Put(command);
//                            return;
//                        }
//                        command.StatusCode = responseStatusCode;
//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = updateResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine(" put {0} response {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(command.OnSuccess);
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Put request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }
//        public async void Put<T>(POSTCommand<T> command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var putResponse = await _client.PutAsync(command.UrlWithToken(_loginResponse.AccessToken),
//                    new StringContent(command.PostBody, Encoding.UTF8, "application/json"));

//                if (!putResponse.IsSuccessStatusCode)
//                {
//                    if (putResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Put(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {putResponse.StatusCode} at PUT<T> {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Put(command);
//                            return;
//                        }

//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = putResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine("\n\n url {0}\nresponse {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(
//                        () => command.OnSuccess(JsonConvert.DeserializeObject<T>(responseString)));
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Post request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        public async void Delete(RESTCommand command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var deleteResponse = await _client.DeleteAsync(command.UrlWithToken(_loginResponse.AccessToken));

//                if (!deleteResponse.IsSuccessStatusCode)
//                {
//                    if (deleteResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Delete(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {deleteResponse.StatusCode} at DELETE {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Delete(command);
//                            return;
//                        }

//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = deleteResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine(" delete {0} response {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(command.OnSuccess);
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Delete request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        public async void Delete<T>(RESTCommand<T> command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var deleteResponse = await _client.DeleteAsync(command.UrlWithToken(_loginResponse.AccessToken));

//                if (!deleteResponse.IsSuccessStatusCode)
//                {
//                    if (deleteResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Delete(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {deleteResponse.StatusCode} at DELETE<T> {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Delete(command);
//                            return;
//                        }

//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = deleteResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine(" delete {0} response {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(
//                        () => command.OnSuccess(JsonConvert.DeserializeObject<T>(responseString)));
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Delete request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        public async void Get<T>(RESTCommand<T> command, bool authorized = true)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var endpoint = authorized ? command.UrlWithToken(_loginResponse.AccessToken) : command.EndPoint;
//                var getResponse = await _client.GetAsync(endpoint);

//                if (!getResponse.IsSuccessStatusCode)
//                {
//                    if (getResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        System.Diagnostics.Debug.WriteLine("Refresh");
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            Get(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Network Error {getResponse.StatusCode} at GET<T> {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            Get(command);
//                            return;
//                        }

//                        command.OnError();
//                    });
//                    return;
//                }
//                using (var responseContent = getResponse.Content)
//                {
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//                    System.Diagnostics.Debug.WriteLine("\n\n url {0}\nresponse {1}", command.EndPoint,
//                        responseString);
//                    Device.BeginInvokeOnMainThread(
//                        () => command.OnSuccess(JsonConvert.DeserializeObject<T>(responseString)));
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("Get request time out: {0}", e.Message);
//                if (_client != null)
//                {
//                    Device.BeginInvokeOnMainThread(command.OnError);
//                }
//            }
//        }

//        private HttpClient GetNewHttpClient()
//        {
//            return new HttpClient(new NativeMessageHandler()) { BaseAddress = new Uri(API_ROOT) };
//        }
//    }
//}
