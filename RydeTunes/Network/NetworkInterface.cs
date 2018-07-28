//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using ModernHttpClient;
//using NeedsGap.Core;
//using NeedsGap.Extensions;
//using NeedsGap.GeneralDomains;
//using Newtonsoft.Json;
//using Plugin.Media.Abstractions;
//using Xamarin.Forms;
//using PCLAppConfig;
//using Plugin.Connectivity;

//namespace NeedsGap.NetworkServices
//{
//    public class NetworkInterface
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

//        private string API_ROOT => ConfigurationManager.AppSettings["API_ROOT"];

//        private static string GoogleAddressApiRoot = "https://maps.googleapis.com/maps/api/";
//        private static string GoogleAddressApiKey = "AIzaSyDOgozNT1BLp_evsHy0P1pfn2Otmem_2p4";
//        private static string LOGIN_ENDPOINT = "oauth/token";

//        private HttpClient _client;
//        private LoginResponse _loginResponse;
//        private bool _hasAttempted;

//        public NetworkInterface()
//        {
//            _client = GetNewHttpClient();
//            CrossConnectivity.Current.ConnectivityChanged += OnDeviceConnectivityChanged;
//        }

//        public void LogOut()
//        {
//            _client.CancelPendingRequests();
//            _client.Dispose();
//            _client = null;
//        }

//        public void Login(string username, string password, Action<LoginResult> onComplete, Action<string> onNetworkError)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                onNetworkError(DeviceNetworkConnectivityErrorMessage);
//                return;
//            }

//            new Task(() =>
//            {
//                try
//                {
//                    using (var client = GetNewHttpClient())
//                    {
//                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", $"{"needs-gap-client:secret".ToBase64String()}");
//                        var authParameters =
//                            $"username={username.ToBase64String()}&password={password.ToBase64String()}&grant_type=password";

//                        var url = $"{LOGIN_ENDPOINT}?{authParameters}";
//                        var postLogin = client.SendAsync(new HttpRequestMessage(HttpMethod.Post, url));

//                        postLogin.Wait();

//                        if (postLogin.IsCompleted)
//                        {
//                            using (var response = postLogin.Result)
//                            {
//                                if (!response.IsSuccessStatusCode)
//                                {
//                                    onComplete(LoginResult.Failure);
//                                    return;
//                                }
//                                using (var responseContent = response.Content)
//                                {
//                                    _hasAttempted = false;
//                                    var responseString = responseContent.ReadAsStringAsync().Result;
//                                    System.Diagnostics.Debug.WriteLine("Login Response {0}", responseString);
//                                    _loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseString);
//                                    System.Diagnostics.Debug.WriteLine("AccessToken = {0}", _loginResponse.AccessToken);

//                                    DependencyService.Get<IAnalyticsTracker>().SendLogin();

//                                    onComplete(LoginResult.Success);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            Device.BeginInvokeOnMainThread(() => onNetworkError("Network error occurred."));
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    System.Diagnostics.Debug.WriteLine("Network failed: {0}", e.Message);
//                    onNetworkError(e.Message);
//                }
//            }).Start();
//        }

//        private object _refreshLock = new object();
//        private bool _refreshing;

//        private struct RefreshActionQueue
//        {
//            public Action OnSuccess;
//            public Action OnError;
//        };

//        private Queue<RefreshActionQueue> _refreshActionQueue = new Queue<RefreshActionQueue>();

//        private async void Refresh(Action onSuccess, Action onError)
//        {
//            lock (_refreshLock)
//            {
//                System.Diagnostics.Debug.WriteLine("Got the refresh lock.");
//                _refreshActionQueue.Enqueue(new RefreshActionQueue { OnSuccess = onSuccess, OnError = onError });
//                if (_refreshing)
//                {
//                    System.Diagnostics.Debug.WriteLine("Returning because it's refreshing.");
//                    return;
//                }

//                _refreshing = true;
//            }
//            System.Diagnostics.Debug.WriteLine("Starting the refresh request.");

//            var authParameters = $"grant_type=refresh_token&refresh_token={_loginResponse.RefreshToken}";

//            using (var client = GetNewHttpClient())
//            {
//                if (!CrossConnectivity.Current.IsConnected)
//                {
//                    await OnNoDeviceNetworkConnectivity();
//                    RefreshProcessQueues(isSuccess: false);
//                    return;
//                }

//                client.DefaultRequestHeaders.Add("Authorization", $"Basic {"needs-gap-client:secret".ToBase64String()}");

//                using (var response = client.PostAsync(
//                    LOGIN_ENDPOINT,
//                    new StringContent(authParameters, Encoding.UTF8, "application/x-www-form-urlencoded")).Result)
//                {
//                    if (!response.IsSuccessStatusCode)
//                    {
//                        if (response.StatusCode == HttpStatusCode.BadRequest &&
//                            NeedsGapApi.Instance.CanAutoLogIn && !_hasAttempted)
//                        {
//                            _hasAttempted = true;
//                            System.Diagnostics.Debug.WriteLine("We can try logging in again");
//                            NeedsGapApi.Instance.LogInWithStoredCredentials(RefreshOnSuccess, _ => RefreshOnError());
//                            return;
//                        }
//                        System.Diagnostics.Debug.WriteLine("Refresh failed with {0}", response.StatusCode);
//                        RefreshOnError();
//                        return;
//                    }

//                    using (var responseContent = response.Content)
//                    {
//                        var responseString = responseContent.ReadAsStringAsync().Result;
//                        _loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseString);
//                        System.Diagnostics.Debug.WriteLine("Refresh AccessToken = {0}", _loginResponse.AccessToken);
//                        Device.BeginInvokeOnMainThread(RefreshOnSuccess);
//                    }
//                }
//            }
//        }

//        public void RefreshOnSuccess()
//        {
//            RefreshProcessQueues(isSuccess: true);
//        }

//        public async void RefreshOnError()
//        {
//            RefreshProcessQueues(isSuccess: false);
//            await Application.Current.MainPage.DisplayAlert("Network Error", "Could not contact the server, please try again later.", "OK");
//        }

//        private void RefreshProcessQueues(bool isSuccess)
//        {
//            int queueLength;

//            lock (_refreshLock)
//            {
//                queueLength = _refreshActionQueue.Count;
//            }

//            while (queueLength > 0)
//            {
//                Action action;

//                lock (_refreshLock)
//                {
//                    var refreshAction = _refreshActionQueue.Dequeue();
//                    action = isSuccess ? refreshAction.OnSuccess : refreshAction.OnError;
//                    queueLength = _refreshActionQueue.Count;
//                }

//                action();
//            }

//            lock (_refreshLock)
//            {
//                _refreshing = false;
//            }
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
//                    DependencyService.Get<IAnalyticsTracker>().SendNetworkError("POST", command.EndPoint, postResponse.StatusCode.ToString());
//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        if (shouldRetry)
//                        {
//                            DependencyService.Get<IAnalyticsTracker>().SendNetworkError("GET", endpoint, postResponse.StatusCode.ToString());
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("POST", command.EndPoint, postResponse.StatusCode.ToString());
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("POST", command.EndPoint, postResponse.StatusCode.ToString());
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

//        public async void PostMediaFile(POSTCommand<string> command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                var mediaFileToSend = (MediaFile)command.PostObject;
//                var imageContent = new ByteArrayContent(PhotoService.GetPhotoMediaFileByteString(mediaFileToSend));
//                imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
//                {
//                    Name = "file",
//                    FileName = Path.GetFileName(mediaFileToSend.Path)
//                };

//                var form = new MultipartFormDataContent("boundaryValue") { imageContent };

//                form.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
//                form.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", "boundaryValue"));

//                var postResponse = await _client.PostAsync(command.UrlWithToken(_loginResponse.AccessToken), form);

//                if (!postResponse.IsSuccessStatusCode)
//                {
//                    if (postResponse.StatusCode == HttpStatusCode.Unauthorized && !command.WasRefreshed)
//                    {
//                        Refresh(() =>
//                        {
//                            command.WasRefreshed = true;
//                            PostMediaFile(command);
//                        }, command.OnError);
//                        return;
//                    }

//                    Device.BeginInvokeOnMainThread(async () =>
//                    {
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("POST", command.EndPoint,
//                                                                                    postResponse.StatusCode.ToString());
//                        System.Diagnostics.Debug.WriteLine($"Network Error {postResponse.StatusCode} at POSTMediaFile {command.EndPoint}");
//                        var retry = await Application.Current.MainPage.DisplayAlert("Network Error",
//                            "Would you like to retry?", "Yes", "No");

//                        if (retry)
//                        {
//                            command.WasRefreshed = false;
//                            PostMediaFile(command);
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
//                    Device.BeginInvokeOnMainThread(
//                        () => command.OnSuccess(JsonConvert.DeserializeObject<string>(responseString)));
//                }
//            }
//            catch (TaskCanceledException e)
//            {
//                System.Diagnostics.Debug.WriteLine($"The task was cancelled. {e.Message}");
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine("PostMediaFile request time out: {0}", e.Message);
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("PUT", command.EndPoint, updateResponse.StatusCode.ToString());
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("PUT", command.EndPoint, putResponse.StatusCode.ToString());
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("DELETE", command.EndPoint, deleteResponse.StatusCode.ToString());
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("DELETE", command.EndPoint, deleteResponse.StatusCode.ToString());
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
//                        DependencyService.Get<IAnalyticsTracker>().SendNetworkError("GET", command.EndPoint, getResponse.StatusCode.ToString());
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

//        public async void GetAddressInfo<T>(RESTCommand<T> command)
//        {
//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                await OnNoDeviceNetworkConnectivity();
//                command.OnError();
//                return;
//            }

//            try
//            {
//                using (var client = new HttpClient { BaseAddress = new Uri(GoogleAddressApiRoot) })
//                {
//                    var getResponse = await client.GetAsync($"{command.EndPoint}&key={GoogleAddressApiKey}");

//                    if (!getResponse.IsSuccessStatusCode)
//                    {
//                        System.Diagnostics.Debug.WriteLine("Error getting address recommendations");
//                        Device.BeginInvokeOnMainThread(command.OnError);
//                        return;
//                    }

//                    using (var responseContent = getResponse.Content)
//                    {
//                        var responseString = responseContent.ReadAsStringAsync().Result;
//                        System.Diagnostics.Debug.WriteLine("\n\n url {0}\nresponse {1}", command.EndPoint,
//                            responseString);
//                        Device.BeginInvokeOnMainThread(
//                            () => command.OnSuccess(JsonConvert.DeserializeObject<T>(responseString)));
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                System.Diagnostics.Debug.WriteLine(
//                    $"Exception occurred while geting address recommendations: {e.Message}");
//                Device.BeginInvokeOnMainThread(command.OnError);
//            }
//        }

//        private HttpClient GetNewHttpClient()
//        {
//            return new HttpClient(new NativeMessageHandler()) { BaseAddress = new Uri(API_ROOT) };
//        }

//        private async Task OnNoDeviceNetworkConnectivity()
//        {
//            NeedsGapApi.Instance.IsConnectedToNetwork = false;
//            if (Application.Current.MainPage != null)
//            {
//                await Application.Current.MainPage.DisplayAlert(NetworkErrorTitle, DeviceNetworkConnectivityErrorMessage, "OK");
//            }
//        }

//        private async void OnDeviceConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
//        {
//            NeedsGapApi.Instance.IsConnectedToNetwork = e.IsConnected;
//            if (!e.IsConnected)
//            {
//                await Application.Current.MainPage.DisplayAlert(
//                    NetworkErrorTitle,
//                    "Your device just lost its connection to the network. Please connect and reload the page.",
//                    "OK");
//            }
//        }
//    }
//}
