﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ServingFresh.Config;
using ServingFresh.Effects;
using ServingFresh.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System.Diagnostics;
using Plugin.LatestVersion;
using Xamarin.Auth;
using ServingFresh.LogIn.Apple;
using Acr.UserDialogs;
using ServingFresh.LogIn.Classes;
using System.Text;
using System.Threading.Tasks;
using static ServingFresh.Views.ItemsPage;
using System.IO;
using System.ComponentModel;

namespace ServingFresh.Views
{
    public partial class SelectionPage : ContentPage
    {
        class BusinessCard
        {
            public string business_image { get; set; }
            public string business_name { get; set; }
            //public string item_type { get; set; }
            public string business_uid { get; set; }
            //public string business_type { get; set; }
            public IDictionary<string,IList<string>> list_delivery_days { get; set; }
            public Color border_color { get; set; }
        }


        BusinessCard unselectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                //item_type = b.item_type,
                business_uid = b.z_biz_id,
                //business_type = b.business_type,
                list_delivery_days = b.delivery_days,
                border_color = Color.LightGray
            };
        }

        BusinessCard selectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                //item_type = b.item_type,
                business_uid = b.z_biz_id,
                //business_type = b.business_type,
                list_delivery_days = b.delivery_days,
                border_color = Constants.SecondaryColor
            };
        }

        public class AcceptingSchedule
        {
            public IList<string> Friday { get; set; }
            public IList<string> Monday { get; set; }
            public IList<string> Sunday { get; set; }
            public IList<string> Tuesday { get; set; }
            public IList<string> Saturday { get; set; }
            public IList<string> Thursday { get; set; }
            public IList<string> Wednesday { get; set; }
        }

        List<DeliveriesModel> AllDeliveries = new List<DeliveriesModel>();
        List<Business> AllFarms = new List<Business>();
        List<Business> AllFarmersMarkets = new List<Business>();
        List<Business> OpenFarms = new List<Business>();

        ObservableCollection<DeliveriesModel> Deliveries = new ObservableCollection<DeliveriesModel>();
        ObservableCollection<BusinessCard> Farms = new ObservableCollection<BusinessCard>();
        ObservableCollection<BusinessCard> FarmersMarkets = new ObservableCollection<BusinessCard>();

        List<string> types = new List<string>();
        List<string> b_uids = new List<string>();
        string selected_market_id = "";
        string selected_farm_id = "";

        ScheduleInfo selectedDeliveryDate;

        public class DeliveryInfo
        {
            public string delivery_day { get; set; }
            public string delivery_time { get; set; }
        }

        public class Delivery
        {
            public string delivery_date { get; set; }
            public string delivery_shortname { get; set; }
            public string delivery_dayofweek { get; set; }
            public string delivery_time { get; set; }
        }

        public class ScheduleInfo : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public string delivery_date { get; set; }
            public string delivery_shortname { get; set; }
            public string delivery_dayofweek { get; set; }
            public string delivery_time { get; set; }
            public List<string> business_uids { get; set; }
            public DateTime deliveryTimeStamp { get; set; }
            public string orderExpTime { get; set; }
            public Xamarin.Forms.Color colorSchedule { get; set; }
            public Xamarin.Forms.Color textColor { get; set; }

            public Xamarin.Forms.Color colorScheduleUpdate {
                 set
                {
                    colorSchedule = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("colorSchedule"));
                }
            }

            public Xamarin.Forms.Color textColorUpdate
            {
                set
                {
                    textColor = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("textColor"));
                }
            }
        }

        public class Filter : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public string filterName { get; set; }
            public string iconSource { get; set; }
            public bool isSelected { get; set; }
            public string type { get; set; }

            public Filter(string filterName, string iconSource, string type)
            {
                this.filterName = filterName;
                this.iconSource = iconSource;
                this.type = type;
                isSelected = false;
            }

            public string updateImage
            {
                set
                {
                    iconSource = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("iconSource"));
                }
            }

        }

        GridLength columnWidth;
        ObservableCollection<Filter> filters;
        List<DeliveryInfo> deliveryDays = new List<DeliveryInfo>();
        List<string> deliveryDayList = new List<string>();
        List<DeliveriesModel> deliveryScheduleUnfiltered = new List<DeliveriesModel>();
        List<Delivery> deliveryScheduleFiltered = new List<Delivery>();
        List<ScheduleInfo> schedule = new List<ScheduleInfo>();
        List<string> businessList = new List<string>();
        List<BusinessCard> businesses = new List<BusinessCard>();
        List<BusinessCard> business = new List<BusinessCard>();
        List<ScheduleInfo> copy = new List<ScheduleInfo>();
        List<DateTime> deliverySchedule = new List<DateTime>();
        List<ScheduleInfo> displaySchedule = new List<ScheduleInfo>();
        ServingFreshBusiness data = new ServingFreshBusiness();
        private string deviceId;
        List<Items> data2 = new List<Items>();
        public ObservableCollection<ItemsModel> datagrid = new ObservableCollection<ItemsModel>();
        public IDictionary<string, ItemPurchased> order = new Dictionary<string, ItemPurchased>();
        public int totalCount = 0;
        // THIS SELECTION PAGE IS USE IN ALL LOGINS ONLY

        //public DeliveriesPage(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs e = null, AppleAccount account = null, string platform = "")
        //{
        //    InitializeComponent();
        //    UserDialogs.Instance.ShowLoading("Please wait while we are processing your request...");
        //    SetHeightWidthOnMap();
        //    SetWidthOnHelpButtonRow();
        //    SetDefaultLocationOnMap();
        //    BackupDisplay.Margin = new Thickness(0, Application.Current.MainPage.Height, 0, 0);
        //    if (platform == "GOOGLE")
        //    {
        //        VerifyUserAccount(accessToken, refreshToken, e, null, "GOOGLE");
        //    }
        //    else if (platform == "FACEBOOK")
        //    {
        //        VerifyUserAccount(accessToken, "", null, null, "FACEBOOK");
        //    }
        //    else if (platform == "APPLE")
        //    {
        //        VerifyUserAccount(account.UserId, "", null, account, "APPLE");
        //    }
        //}

        public SelectionPage(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleCredentials = null, AppleAccount appleCredentials = null, string platform = "")
        {
            InitializeComponent();
            _ = VerifyUserCredentials(accessToken, refreshToken, googleCredentials, appleCredentials, platform);
        }

        static async Task WaitAndApologizeAsync()
        {
            await Task.Delay(2000);
        }

        public async Task VerifyUserCredentials(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleAccount = null, AppleAccount appleCredentials = null, string platform= "")
        {
            try
            {
                //var progress = UserDialogs.Instance.Loading("Loading...");
                var client = new HttpClient();
                var socialLogInPost = new SocialLogInPost();

                var googleData = new GoogleResponse();
                var facebookData = new FacebookResponse();

                if (platform == "GOOGLE")
                {
                    var request = new OAuth2Request("GET", new Uri(Constant.GoogleUserInfoUrl), null, googleAccount.Account);
                    var GoogleResponse = await request.GetResponseAsync();
                    var googelUserData = GoogleResponse.GetResponseText();

                    googleData = JsonConvert.DeserializeObject<GoogleResponse>(googelUserData);

                    socialLogInPost.email = googleData.email;
                    socialLogInPost.social_id = googleData.id;
                }
                else if (platform == "FACEBOOK")
                {
                    var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
                    var facebookUserData = facebookResponse.Result;

                    facebookData = JsonConvert.DeserializeObject<FacebookResponse>(facebookUserData);

                    socialLogInPost.email = facebookData.email;
                    socialLogInPost.social_id = facebookData.id;
                }
                else if (platform == "APPLE")
                {
                    socialLogInPost.email = appleCredentials.Email;
                    socialLogInPost.social_id = appleCredentials.UserId;
                }

                socialLogInPost.password = "";
                socialLogInPost.signup_platform = platform;

                var socialLogInPostSerialized = JsonConvert.SerializeObject(socialLogInPost);
                var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");

                var test = UserDialogs.Instance.Loading("Loading...");
                var RDSResponse = await client.PostAsync(Constant.LogInUrl, postContent);
                var responseContent = await RDSResponse.Content.ReadAsStringAsync();
                var authetication = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                if (RDSResponse.IsSuccessStatusCode)
                {
                    if (responseContent != null)
                    {
                        if (authetication.code.ToString() == Constant.EmailNotFound)
                        {
                            test.Hide();
                            if(platform == "GOOGLE")
                            {
                                Application.Current.MainPage = new SocialSignUp(googleData.id, googleData.given_name, googleData.family_name, googleData.email, accessToken, refreshToken, "GOOGLE");
                            }
                            else if (platform == "FACEBOOK")
                            {
                                Application.Current.MainPage = new SocialSignUp(facebookData.id, facebookData.name, "", facebookData.email, accessToken, accessToken, "FACEBOOK");
                            }
                            else if (platform == "APPLE")
                            {
                                Application.Current.MainPage = new SocialSignUp(appleCredentials.UserId, appleCredentials.Name, "", appleCredentials.Email, appleCredentials.Token, appleCredentials.Token, "APPLE");
                            }
                        }
                        if (authetication.code.ToString() == Constant.AutheticatedSuccesful)
                        {
                            try
                            {
                                var data = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                                Application.Current.Properties["user_id"] = data.result[0].customer_uid;

                                UpdateTokensPost updateTokesPost = new UpdateTokensPost();
                                updateTokesPost.uid = data.result[0].customer_uid;
                                if (platform == "GOOGLE")
                                {
                                    updateTokesPost.mobile_access_token = accessToken;
                                    updateTokesPost.mobile_refresh_token = refreshToken;
                                }
                                else if (platform == "FACEBOOK")
                                {
                                    updateTokesPost.mobile_access_token = accessToken;
                                    updateTokesPost.mobile_refresh_token = accessToken;
                                }else if (platform == "APPLE")
                                {
                                    updateTokesPost.mobile_access_token = appleCredentials.Token;
                                    updateTokesPost.mobile_refresh_token = appleCredentials.Token;
                                }

                                var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                                var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                                var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);
                                var updateTokenResponseContent = await updateTokesResponse.Content.ReadAsStringAsync();

                                if (updateTokesResponse.IsSuccessStatusCode)
                                {
                                    var user = new RequestUserInfo();
                                    user.uid = data.result[0].customer_uid;

                                    var requestSelializedObject = JsonConvert.SerializeObject(user);
                                    var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                                    var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                                    if (clientRequest.IsSuccessStatusCode)
                                    {
                                        var userSfJSON = await clientRequest.Content.ReadAsStringAsync();
                                        var userProfile = JsonConvert.DeserializeObject<UserInfo>(userSfJSON);

                                        DateTime today = DateTime.Now;
                                        DateTime expDate = today.AddDays(Constant.days);

                                        Application.Current.Properties["user_id"] = data.result[0].customer_uid;
                                        Application.Current.Properties["time_stamp"] = expDate;
                                        Application.Current.Properties["platform"] = platform;
                                        Application.Current.Properties["user_email"] = userProfile.result[0].customer_email;
                                        Application.Current.Properties["user_first_name"] = userProfile.result[0].customer_first_name;
                                        Application.Current.Properties["user_last_name"] = userProfile.result[0].customer_last_name;
                                        Application.Current.Properties["user_phone_num"] = userProfile.result[0].customer_phone_num;
                                        Application.Current.Properties["user_address"] = userProfile.result[0].customer_address;
                                        Application.Current.Properties["user_unit"] = userProfile.result[0].customer_unit;
                                        Application.Current.Properties["user_city"] = userProfile.result[0].customer_city;
                                        Application.Current.Properties["user_state"] = userProfile.result[0].customer_state;
                                        Application.Current.Properties["user_zip_code"] = userProfile.result[0].customer_zip;
                                        Application.Current.Properties["user_latitude"] = userProfile.result[0].customer_lat;
                                        Application.Current.Properties["user_longitude"] = userProfile.result[0].customer_long;

                                        _ = Application.Current.SavePropertiesAsync();
                                        await CheckVersion();

                                        if (Device.RuntimePlatform == Device.iOS)
                                        {
                                            deviceId = Preferences.Get("guid", null);
                                            if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
                                        }
                                        else
                                        {
                                            deviceId = Preferences.Get("guid", null);
                                            if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
                                        }

                                        if (deviceId != null)
                                        {
                                            NotificationPost notificationPost = new NotificationPost();

                                            notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                            notificationPost.guid = deviceId.Substring(5);
                                            Application.Current.Properties["guid"] = deviceId.Substring(5);
                                            notificationPost.notification = "TRUE";

                                            var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                            Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                            var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                            var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                            Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                                            if (clientResponse.IsSuccessStatusCode)
                                            {
                                                System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                            }
                                            else
                                            {
                                                await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                            }
                                        }
                                        test.Hide();
                                        //Application.Current.MainPage = new SelectionPage();
                                    }
                                    else
                                    {
                                        test.Hide();
                                        await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                                    }
                                }
                                else
                                {
                                    test.Hide();
                                    await DisplayAlert("Oops", "We are facing some problems with our internal system. We weren't able to update your credentials", "OK");
                                }
                                test.Hide();
                            }
                            catch (Exception second)
                            {
                                Debug.WriteLine(second.Message);
                            }
                        }
                        if (authetication.code.ToString() == Constant.ErrorPlatform)
                        {
                            var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);
                            test.Hide();
                            Application.Current.MainPage = new LogInPage("Message", RDSCode.message);
                        }

                        if (authetication.code.ToString() == Constant.ErrorUserDirectLogIn)
                        {
                            test.Hide();
                            Application.Current.MainPage = new LogInPage("Oops!", "You have an existing Serving Fresh account. Please use direct login");
                        }
                    }
                }
                test.Hide();
            }
            catch (Exception first)
            {
                Debug.WriteLine(first.Message);
            }
        }


        public SelectionPage()
        {
            InitializeComponent();
            _ = CheckVersion();
        }

        public async Task CheckVersion()
        {
            var isLatest = await CrossLatestVersion.Current.IsUsingLatestVersion();
            
            if (!isLatest)
            {
                await DisplayAlert("Serving Fresh\nhas gotten even better!", "Please visit the App Store to get the latest version.", "OK");
                await CrossLatestVersion.Current.OpenAppInStore();
            }
            else
            {
                 GetBusinesses();
                Application.Current.Properties["zone"] = "";
                Application.Current.Properties["day"] = "";
                Application.Current.Properties["deliveryDate"] = "";
                CartTotal.Text = CheckoutPage.total_qty.ToString();
            }
        }

        public SelectionPage(Location current)
        {
            InitializeComponent();
            columnWidth = deliveryDatesColumn.Width;

            filters = new ObservableCollection<Filter>();



            filters.Add(new Filter("Fruits", "unselectedFruitsIcon.png", "fruit"));
            filters.Add(new Filter("Vegetables", "unselectedVegetablesIcon.png", "vegetable"));
            filters.Add(new Filter("Desserts", "unselectedDessertsIcon.png", "dessert"));
            filters.Add(new Filter("Others", "unselectedOthersIcon.png", "other"));
            filters.Add(new Filter("Favorites", "unselectedFavoritesIcon.png" , "favorite"));


            filterList.ItemsSource = filters;
            Application.Current.Properties["user_latitude"] = current.Latitude.ToString();
            Application.Current.Properties["user_longitude"] = current.Longitude.ToString();

            Debug.WriteLine("INPUT 1 (SelectionPage): " + current.Latitude);
            Debug.WriteLine("INPUT 2 (SelectionPage): " + current.Longitude);

            GetBusinesses();
        }

        public async void GetBusinesses()
        {
            var userLat = (string)Application.Current.Properties["user_latitude"];
            var userLong = (string)Application.Current.Properties["user_longitude"];

            Debug.WriteLine("INPUT 1: " + userLat);
            Debug.WriteLine("INPUT 2: " + userLong);

            //if (userLat == "0" && userLong == "0"){ userLong = "-121.8866517"; userLat = "37.2270928";}

            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ProduceByLocation + userLong + "," + userLat);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("CALL TO ENDPOINT SUCCESSFULL?: " + response.IsSuccessStatusCode);
            Debug.WriteLine("JSON RETURNED: " + result);

            if (response.IsSuccessStatusCode)
            {
                data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);

                var currentDate = DateTime.Now;
                var tempDateTable = GetTable(currentDate);

                foreach (Business a in data.business_details)
                {
                    var acceptingDate = LastAcceptingOrdersDate(tempDateTable, a.z_accepting_day, a.z_accepting_time);
                    var deliveryDate = new DateTime();
                    //Debug.WriteLine("CURRENT DATE: " + currentDate);
                    //Debug.WriteLine("LAST ACCEPTING DATE: " + acceptingDate);

                    if (currentDate < acceptingDate)
                    {
                        //Debug.WriteLine("ONTIME");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);
                        if (deliveryDate < acceptingDate)
                        {
                            deliveryDate = deliveryDate.AddDays(7);
                        }
                        if (!deliverySchedule.Contains(deliveryDate))
                        {
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = deliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = deliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = deliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = deliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.ToString("ddd") + " " + acceptingDate.ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.WriteLine("business_uids list: ");

                            //foreach(string ID in element.business_uids)
                            //{
                            //    Debug.WriteLine(ID);
                            //}

                            deliverySchedule.Add(deliveryDate);
                            displaySchedule.Add(element);
                        }
                        else
                        {
                            foreach (ScheduleInfo element in displaySchedule)
                            {
                                if (element.deliveryTimeStamp == deliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.WriteLine("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.WriteLine(ID);
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("NON ONTIME! -> ROLL OVER TO NEXT DELIVERY DATE");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);

                        if (!deliverySchedule.Contains(deliveryDate.AddDays(7)))
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = nextDeliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = nextDeliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = nextDeliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = nextDeliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.AddDays(7).ToString("ddd") + " " + acceptingDate.AddDays(7).ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.Write("business_uids list: ");

                            //foreach (string ID in element.business_uids)
                            //{
                            //    Debug.Write(ID + ", ");
                            //}

                            deliverySchedule.Add(nextDeliveryDate);
                            displaySchedule.Add(element);
                        }
                        else
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);

                            foreach (ScheduleInfo element in displaySchedule)
                            {
                                if (element.deliveryTimeStamp == nextDeliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.Write("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.Write(ID + ", ");
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                }

                deliverySchedule.Sort();
                List<ScheduleInfo> sortedSchedule = new List<ScheduleInfo>();

                foreach (DateTime deliveryElement in deliverySchedule)
                {
                    foreach (ScheduleInfo scheduleElement in displaySchedule)
                    {
                        if (deliveryElement == scheduleElement.deliveryTimeStamp)
                        {
                            sortedSchedule.Add(scheduleElement);
                        }
                    }
                }

                displaySchedule = sortedSchedule;

                if (data.business_details.Count != 0)
                {
                    // Parse it
                    Application.Current.Properties["zone"] = data.business_details[0].zone;

                    deliveryDays.Clear();
                    businessList.Clear();

                    foreach (Business business in data.business_details)
                    {
                        DeliveryInfo element = new DeliveryInfo();
                        element.delivery_day = business.z_delivery_day;
                        element.delivery_time = business.z_delivery_time;

                        bool addElement = false;

                        if (deliveryDays.Count != 0)
                        {
                            foreach (DeliveryInfo i in deliveryDays)
                            {
                                if (element.delivery_time == i.delivery_time && element.delivery_day == i.delivery_day)
                                {
                                    addElement = true;
                                    break;
                                }
                            }
                            if (!addElement)
                            {
                                deliveryDays.Add(element);
                            }
                        }
                        else
                        {
                            deliveryDays.Add(element);
                        }

                        if (!businessList.Contains(business.z_biz_id))
                        {
                            businessList.Add(business.z_biz_id);
                        }
                    }

                    foreach (string id in businessList)
                    {
                        //Debug.WriteLine(id + " :");
                        IDictionary<string, IList<string>> days = new Dictionary<string, IList<string>>();
                        foreach (Business b in data.business_details)
                        {
                            if (id == b.z_biz_id)
                            {
                                if (!days.ContainsKey(b.z_delivery_day.ToUpper()))
                                {
                                    IList<string> times = new List<string>();
                                    times.Add(b.z_delivery_time);
                                    days.Add(b.z_delivery_day.ToUpper(), times);
                                }
                                else
                                {
                                    List<string> times = (List<string>)days[b.z_delivery_day.ToUpper()];
                                    times.Add(b.z_delivery_time);
                                    days[b.z_delivery_day.ToUpper()] = times;
                                }

                            }
                        }

                        foreach (Business i in data.business_details)
                        {
                            if (id == i.z_biz_id)
                            {
                                i.delivery_days = days;
                                businesses.Add(unselectedBusiness(i));
                                break;
                            }
                        }
                    }

                    foreach(ScheduleInfo i in displaySchedule)
                    {
                        i.colorSchedule = Color.FromHex("#E0E6E6");
                        i.textColor = Color.FromHex("#136D74");
                    }

                    delivery_list.ItemsSource = displaySchedule;

                    if (displaySchedule.Count != 0)
                    {
                        selectedDeliveryDate = displaySchedule[0];
                        displaySchedule[0].colorScheduleUpdate = Color.FromHex("#FF8500");
                        displaySchedule[0].textColorUpdate = Color.FromHex("#FFFFFF"); 
                        var day = DateTime.Parse(displaySchedule[0].deliveryTimeStamp.ToString());
                        title.Text = day.ToString("ddd") + ", " + displaySchedule[0].delivery_date.ToString();
                        deliveryTime.Text = displaySchedule[0].delivery_time;
                        orderBy.Text = displaySchedule[0].orderExpTime;
                        Debug.WriteLine("NUMBER OF ITEMS PASSED TO PARSE FUNCTION: " + data.result.Count);
                        GetData(data.result);
                        //var types = new List<string>();
                        //types.Add("fruit");
                        //types.Add("vegetable");
                        //types.Add("dessert");
                        //types.Add("other");

                        //GetData(types, displaySchedule[0].business_uids);
                        Debug.WriteLine("NUMBER OF ITEMS RETURNED BY PARSE FUNCTION " + datagrid.Count);

                        foreach(ItemsModel i in datagrid)
                        {
                            if(!displaySchedule[0].business_uids.Contains(i.itm_business_uidLeft))
                            {
                                i.opacityLeftUpdate = 0.5;
                                i.isItemLeftEnableUpdate = false;
                                i.isItemLeftUnavailableUpdate = true;
                            }
                            else if (!displaySchedule[0].business_uids.Contains(i.itm_business_uidRight))
                            {
                                i.opacityRightUpdate = 0.5;
                                i.isItemRightEnableUpdate = false;
                                i.isItemRightUnavailableUpdate = true;
                            }
                        }
                    }
                    //farm_list.ItemsSource = businesses;
                }
                else
                {
                    await DisplayAlert("Oops", "We don't have a business that can delivery to your location at the moment", "OK");
                    return;
                }
            }
            else
            {
                //await DisplayAlert("Oops!", "Our system is down. We are working to fix this issue.", "OK");
                return;
            }
        }


        private void GetData(IList<Items> listOfItems)
        {
            try
            {
                //GetItemPost post = new GetItemPost();
                //post.type = types;
                //post.ids = b_uids;

                //var client = new HttpClient();
                //var getItemsString = JsonConvert.SerializeObject(post);
                //var getItemsStringMessage = new StringContent(getItemsString, Encoding.UTF8, "application/json");
                //var request = new HttpRequestMessage();
                //request.Method = HttpMethod.Post;
                //request.Content = getItemsStringMessage;

                //var httpResponse = await client.PostAsync(Constant.GetItemsUrl, getItemsStringMessage);
                //var r = await httpResponse.Content.ReadAsStreamAsync();
                //var list = await httpResponse.Content.ReadAsStringAsync();
                //Debug.WriteLine("ITEMS LIST: " + list);
                ////var da = await httpResponse.Content.ReadAsStringAsync();
                ////Debug.WriteLine("PURCHASE: " + da);
                //StreamReader sr = new StreamReader(r);
                //JsonReader reader = new JsonTextReader(sr);

                if (listOfItems.Count != 0 && listOfItems != null)
                {
                    //var serializer = new JsonSerializer();
                    //data2 = serializer.Deserialize<ServingFreshBusinessItems>(items);
                    //data2 = JsonConvert.DeserializeObject<ServingFreshBusinessItems>(items);

                    List<Items> listUniqueItems = new List<Items>();
                    Dictionary<string, Items> uniqueItems = new Dictionary<string, Items>();
                    foreach (Items a in listOfItems)
                    {
                        string key = a.item_name + a.item_desc + a.item_price;
                        if (!uniqueItems.ContainsKey(key))
                        {
                            uniqueItems.Add(key, a);
                        }
                        else
                        {
                            var savedItem = uniqueItems[key];

                            if (savedItem.item_price != a.item_price)
                            {
                                if (savedItem.business_price != Math.Min(savedItem.business_price, a.business_price))
                                {
                                    //savedItem.item_uid = a.item_uid;
                                    savedItem = a;
                                }
                            }
                            else
                            {
                                List<DateTime> creationDates = new List<DateTime>();
                                Debug.WriteLine("NAME {0}, {1}", savedItem.item_name, a.item_name);
                                Debug.WriteLine("SAVED ITEM UID {0}, SAVED TIME STAMP {1}", savedItem.item_uid, savedItem.created_at);
                                Debug.WriteLine("NEW ITEM UID {0}, NEW TIME STAMP {1}", a.item_uid, a.created_at);

                                creationDates.Add(DateTime.Parse(savedItem.created_at));
                                creationDates.Add(DateTime.Parse(a.created_at));
                                creationDates.Sort();

                                if (creationDates[0] != creationDates[1])
                                {
                                    Debug.WriteLine("CREATED FIRST {0}, STRING DATETIME INDEX 0 {1}", creationDates[0], creationDates[0].ToString("yyyy-MM-dd HH:mm:ss"));

                                    if (savedItem.created_at != creationDates[0].ToString("yyyy-MM-dd HH:mm:ss"))
                                    {
                                        //savedItem.item_uid = a.item_uid;
                                        savedItem = a;
                                    }
                                }
                                else
                                {
                                    var itemsIdsList = new List<long>();
                                    var savedItemId = savedItem.item_uid.Replace('-', '0');
                                    var newItemId = a.item_uid.Replace('-', '0');

                                    itemsIdsList.Add(long.Parse(savedItemId));
                                    itemsIdsList.Add(long.Parse(newItemId));
                                    itemsIdsList.Sort();

                                    if (savedItemId != itemsIdsList[0].ToString())
                                    {
                                        //savedItem.item_uid = a.item_uid;
                                        savedItem = a;
                                    }
                                }
                                Debug.WriteLine("SELECTED ITEM UID: " + savedItem.item_uid);
                                uniqueItems[key] = savedItem;
                            }
                        }
                    }

                    foreach (string key in uniqueItems.Keys)
                    {
                        listUniqueItems.Add(uniqueItems[key]);
                    }

                    listOfItems = listUniqueItems;

                    this.datagrid.Clear();
                    int n = listOfItems.Count;
                    int j = 0;
                    if (n == 0)
                    {

                        this.datagrid.Add(new ItemsModel()
                        {
                            height = this.Width / 2 - 10,
                            width = this.Width / 2 - 25,
                            imageSourceLeft = "",
                            quantityLeft = 0,
                            itemNameLeft = "",
                            itemPriceLeft = "$ " + "",
                            itemPriceLeftUnit = "",
                            itemLeftUnit = "",
                            item_businessPriceLeft = 0,
                            isItemLeftVisiable = false,
                            isItemLeftEnable = false,
                            quantityL = 0,
                            item_descLeft = "",
                            itemTaxableLeft = "",
                            colorLeft = Color.FromHex("#FFFFFF"),
                            itemTypeLeft = "",
                            favoriteIconLeft = "unselectedHeartIcon.png",
                            opacityLeft = 0,
                            isItemLeftUnavailable = false,


                            imageSourceRight = "",
                            quantityRight = 0,
                            itemNameRight = "",
                            itemPriceRight = "$ " + "",
                            itemPriceRightUnit = "",
                            itemRightUnit = "",
                            item_businessPriceRight = 0,
                            isItemRightVisiable = false,
                            isItemRightEnable = false,
                            quantityR = 0,
                            item_descRight = "",
                            itemTaxableRight = "",
                            colorRight = Color.FromHex("#FFFFFF"),
                            itemTypeRight = "",
                            favoriteIconRight = "unselectedHeartIcon.png",
                            opacityRight = 0,
                            isItemRightUnavailable = false,
                        });
                    }
                    if (isAmountItemsEven(n))
                    {
                        for (int i = 0; i < n / 2; i++)
                        {
                            if (listOfItems[j].taxable == null || listOfItems[j].taxable == "NULL")
                            {
                                listOfItems[j].taxable = "FALSE";
                            }
                            if (listOfItems[j + 1].taxable == null || listOfItems[j + 1].taxable == "NULL")
                            {
                                listOfItems[j + 1].taxable = "FALSE";
                            }
                            this.datagrid.Add(new ItemsModel()
                            {
                                height = this.Width / 2 - 10,
                                width = this.Width / 2 - 25,
                                imageSourceLeft = listOfItems[j].item_photo,
                                item_uidLeft = listOfItems[j].item_uid,
                                itm_business_uidLeft = listOfItems[j].itm_business_uid,
                                quantityLeft = 0,
                                itemNameLeft = listOfItems[j].item_name,
                                itemPriceLeft = "$ " + listOfItems[j].item_price.ToString(),
                                itemPriceLeftUnit = "$ " + listOfItems[j].item_price.ToString("N2") + " / " + (string)listOfItems[j].item_unit.ToString(),
                                itemLeftUnit = (string)listOfItems[j].item_unit.ToString(),
                                item_businessPriceLeft = listOfItems[j].business_price,
                                isItemLeftVisiable = true,
                                isItemLeftEnable = true,
                                quantityL = 0,
                                item_descLeft = listOfItems[j].item_desc,
                                itemTaxableLeft = listOfItems[j].taxable,
                                colorLeft = Color.FromHex("#FFFFFF"),
                                itemTypeLeft = listOfItems[j].item_type,
                                favoriteIconLeft = "unselectedHeartIcon.png",
                                opacityLeft = 1,
                                isItemLeftUnavailable = false,

                                imageSourceRight = listOfItems[j + 1].item_photo,
                                item_uidRight = listOfItems[j + 1].item_uid,
                                itm_business_uidRight = listOfItems[j + 1].itm_business_uid,
                                quantityRight = 0,
                                itemNameRight = listOfItems[j + 1].item_name,
                                itemPriceRight = "$ " + listOfItems[j + 1].item_price.ToString(),
                                itemPriceRightUnit = "$ " + listOfItems[j + 1].item_price.ToString("N2") + " / " + (string)listOfItems[j + 1].item_unit.ToString(),
                                itemRightUnit = (string)listOfItems[j + 1].item_unit.ToString(),
                                item_businessPriceRight = listOfItems[j + 1].business_price,
                                isItemRightVisiable = true,
                                isItemRightEnable = true,
                                quantityR = 0,
                                item_descRight = listOfItems[j + 1].item_desc,
                                itemTaxableRight = listOfItems[j + 1].taxable,
                                colorRight = Color.FromHex("#FFFFFF"),
                                itemTypeRight = listOfItems[j + 1].item_type,
                                favoriteIconRight = "unselectedHeartIcon.png",
                                opacityRight = 1,
                                isItemRightUnavailable = false,
                            });;
                            j = j + 2;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < n / 2; i++)
                        {
                            if (listOfItems[j].taxable == null || listOfItems[j].taxable == "NULL")
                            {
                                listOfItems[j].taxable = "FALSE";
                            }
                            if (listOfItems[j + 1].taxable == null || listOfItems[j + 1].taxable == "NULL")
                            {
                                listOfItems[j + 1].taxable = "FALSE";
                            }
                            this.datagrid.Add(new ItemsModel()
                            {
                                height = this.Width / 2 - 10,
                                width = this.Width / 2 - 25,
                                imageSourceLeft = listOfItems[j].item_photo,
                                item_uidLeft = listOfItems[j].item_uid,
                                itm_business_uidLeft = listOfItems[j].itm_business_uid,
                                quantityLeft = 0,
                                itemNameLeft = listOfItems[j].item_name,
                                itemPriceLeft = "$ " + listOfItems[j].item_price.ToString(),
                                itemPriceLeftUnit = "$ " + listOfItems[j].item_price.ToString("N2") + " / " + (string)listOfItems[j].item_unit.ToString(),
                                itemLeftUnit = (string)listOfItems[j].item_unit.ToString(),
                                item_businessPriceLeft = listOfItems[j].business_price,
                                isItemLeftVisiable = true,
                                isItemLeftEnable = true,
                                quantityL = 0,
                                item_descLeft = listOfItems[j].item_desc,
                                itemTaxableLeft = listOfItems[j].taxable,
                                colorLeft = Color.FromHex("#FFFFFF"),
                                itemTypeLeft = listOfItems[j].item_type,
                                favoriteIconLeft = "unselectedHeartIcon.png",
                                opacityLeft = 1,
                                isItemLeftUnavailable = false,

                                imageSourceRight = listOfItems[j + 1].item_photo,
                                item_uidRight = listOfItems[j + 1].item_uid,
                                itm_business_uidRight = listOfItems[j + 1].itm_business_uid,
                                quantityRight = 0,
                                itemNameRight = listOfItems[j + 1].item_name,
                                itemPriceRight = "$ " + listOfItems[j + 1].item_price.ToString(),
                                itemPriceRightUnit = "$ " + listOfItems[j + 1].item_price.ToString("N2") + " / " + (string)listOfItems[j + 1].item_unit.ToString(),
                                itemRightUnit = (string)listOfItems[j + 1].item_unit.ToString(),
                                item_businessPriceRight = listOfItems[j + 1].business_price,
                                isItemRightVisiable = true,
                                isItemRightEnable = true,
                                quantityR = 0,
                                item_descRight = listOfItems[j + 1].item_desc,
                                itemTaxableRight = listOfItems[j + 1].taxable,
                                colorRight = Color.FromHex("#FFFFFF"),
                                itemTypeRight = listOfItems[j + 1].item_type,
                                favoriteIconRight = "unselectedHeartIcon.png",
                                opacityRight = 1,
                                isItemRightUnavailable = false,

                            });
                            j = j + 2;
                        }
                        if (listOfItems[j].taxable == null || listOfItems[j].taxable == "NULL")
                        {
                            listOfItems[j].taxable = "FALSE";
                        }
                        this.datagrid.Add(new ItemsModel()
                        {
                            height = this.Width / 2 - 10,
                            width = this.Width / 2 - 25,
                            imageSourceLeft = listOfItems[j].item_photo,
                            item_uidLeft = listOfItems[j].item_uid,
                            itm_business_uidLeft = listOfItems[j].itm_business_uid,
                            quantityLeft = 0,
                            itemNameLeft = listOfItems[j].item_name,
                            itemPriceLeft = "$ " + listOfItems[j].item_price.ToString(),
                            itemPriceLeftUnit = "$ " + listOfItems[j].item_price.ToString("N2") + " / " + (string)listOfItems[j].item_unit.ToString(),
                            itemLeftUnit = (string)listOfItems[j].item_unit.ToString(),
                            item_businessPriceLeft = listOfItems[j].business_price,
                            isItemLeftVisiable = true,
                            isItemLeftEnable = true,
                            quantityL = 0,
                            item_descLeft = listOfItems[j].item_desc,
                            itemTaxableLeft = listOfItems[j].taxable,
                            colorLeft = Color.FromHex("#FFFFFF"),
                            itemTypeLeft = listOfItems[j].item_type,
                            favoriteIconLeft = "unselectedHeartIcon.png",
                            opacityLeft = 1,
                            isItemLeftUnavailable = false,

                            imageSourceRight = "",
                            quantityRight = 0,
                            itemNameRight = "",
                            itemPriceRight = "$ " + "",
                            isItemRightVisiable = false,
                            isItemRightEnable = false,
                            quantityR = 0,
                            colorRight = Color.FromHex("#FFFFFF"),
                            itemTypeRight = "",
                            favoriteIconRight = "unselectedHeartIcon.png",
                            opacityRight = 0,
                            isItemRightUnavailable = false,
                        }) ;
                    }
                }

                foreach (string key in order.Keys)
                {

                    foreach (ItemsModel a in datagrid)
                    {
                        //Debug.WriteLine(orderCopy[key].item_name);
                        //Debug.WriteLine(a.itemNameLeft);
                        if (order[key].item_name == a.itemNameLeft)
                        {
                            Debug.WriteLine(order[key].item_name);
                            a.quantityLeft = order[key].item_quantity;
                            break;
                        }
                        else if (order[key].item_name == a.itemNameRight)
                        {
                            Debug.WriteLine(order[key].item_name);
                            a.quantityRight = order[key].item_quantity;
                            break;
                        }
                    }
                }
                itemList.ItemsSource = datagrid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public bool isAmountItemsEven(int num)
        {
            bool result = false;
            if (num % 2 == 0) { result = true; }
            return result;
        }



        void SubtractItemLeft(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                if (itemModelObject.quantityL != 0)
                {
                    itemModelObject.quantityL -= 1;
                    totalCount -= 1;
                    //CartTotal.Text = totalCount.ToString();
                    
                    if (order != null)
                    {
                        if (order.ContainsKey(itemModelObject.itemNameLeft))
                        {
                            var itemToUpdate = order[itemModelObject.itemNameLeft];
                            itemToUpdate.item_quantity = itemModelObject.quantityL;
                            order[itemModelObject.itemNameLeft] = itemToUpdate;
                        }
                        else
                        {
                            itemSelected.pur_business_uid = itemModelObject.itm_business_uidLeft;
                            itemSelected.item_uid = itemModelObject.item_uidLeft;
                            itemSelected.item_name = itemModelObject.itemNameLeft;
                            itemSelected.item_quantity = itemModelObject.quantityL;
                            itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceLeft.Substring(1).Trim());
                            itemSelected.img = itemModelObject.imageSourceLeft;
                            itemSelected.unit = itemModelObject.itemLeftUnit;
                            itemSelected.description = itemModelObject.item_descLeft;
                            itemSelected.business_price = itemModelObject.item_businessPriceLeft;
                            itemSelected.taxable = itemModelObject.itemTaxableLeft;
                            itemSelected.isItemAvailable = true;
                            order.Add(itemModelObject.itemNameLeft, itemSelected);
                        }
                    }

                    if(itemModelObject.quantityL == 0)
                    {
                        itemModelObject.colorLeft = Color.FromHex("#FFFFFF");
                        itemModelObject.colorLeftUpdate = Color.FromHex("#FFFFFF");
                        order.Remove(itemModelObject.itemNameLeft);
                    }
                    UpdateNumberOfItemsInCart();
                }
                else
                {
                    itemModelObject.colorLeft = Color.FromHex("#FFFFFF");
                    itemModelObject.colorLeftUpdate = Color.FromHex("#FFFFFF");
                }
            }

        }

        void AddItemLeft(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                itemModelObject.colorLeft = Color.FromHex("#ffce99");
                itemModelObject.colorLeftUpdate = Color.FromHex("#ffce99");
                itemModelObject.quantityL += 1;
                totalCount += 1;
                //CartTotal.Text = totalCount.ToString();
                
                if (order != null)
                {
                    if (order.ContainsKey(itemModelObject.itemNameLeft))
                    {
                        var itemToUpdate = order[itemModelObject.itemNameLeft];
                        itemToUpdate.item_quantity = itemModelObject.quantityL;
                        order[itemModelObject.itemNameLeft] = itemToUpdate;
                    }
                    else
                    {
                        itemSelected.pur_business_uid = itemModelObject.itm_business_uidLeft;
                        itemSelected.item_uid = itemModelObject.item_uidLeft;
                        itemSelected.item_name = itemModelObject.itemNameLeft;
                        itemSelected.item_quantity = itemModelObject.quantityL;
                        itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceLeft.Substring(1).Trim());
                        itemSelected.img = itemModelObject.imageSourceLeft;
                        itemSelected.unit = itemModelObject.itemLeftUnit;
                        itemSelected.description = itemModelObject.item_descLeft;
                        itemSelected.business_price = itemModelObject.item_businessPriceLeft;
                        itemSelected.taxable = itemModelObject.itemTaxableLeft;
                        itemSelected.isItemAvailable = true;
                        order.Add(itemModelObject.itemNameLeft, itemSelected);
                    }
                }
                UpdateNumberOfItemsInCart();
            }

        }

        void SubtractItemRight(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                if (itemModelObject.quantityR != 0)
                {
                    itemModelObject.quantityR -= 1;
                    totalCount -= 1;
                    //CartTotal.Text = totalCount.ToString();
                    
                    if (order.ContainsKey(itemModelObject.itemNameRight))
                    {
                        var itemToUpdate = order[itemModelObject.itemNameRight];
                        itemToUpdate.item_quantity = itemModelObject.quantityR;
                        order[itemModelObject.itemNameRight] = itemToUpdate;
                    }
                    else
                    {
                        itemSelected.pur_business_uid = itemModelObject.itm_business_uidRight;
                        itemSelected.item_uid = itemModelObject.item_uidRight;
                        itemSelected.item_name = itemModelObject.itemNameRight;
                        itemSelected.item_quantity = itemModelObject.quantityR;
                        itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceRight.Substring(1).Trim());
                        itemSelected.img = itemModelObject.imageSourceRight;
                        itemSelected.unit = itemModelObject.itemRightUnit;
                        itemSelected.description = itemModelObject.item_descRight;
                        itemSelected.business_price = itemModelObject.item_businessPriceRight;
                        itemSelected.taxable = itemModelObject.itemTaxableRight;
                        itemSelected.isItemAvailable = true;
                        order.Add(itemModelObject.itemNameRight, itemSelected);
                    }

                    if(itemModelObject.quantityR == 0)
                    {
                        itemModelObject.colorRight = Color.FromHex("#FFFFFF");
                        itemModelObject.colorRightUpdate = Color.FromHex("#FFFFFF");
                        order.Remove(itemModelObject.itemNameRight);
                    }
                    UpdateNumberOfItemsInCart();
                }
                else
                {
                    itemModelObject.colorRight = Color.FromHex("#FFFFFF");
                    itemModelObject.colorRightUpdate = Color.FromHex("#FFFFFF");
                }
            }

        }

        void AddItemRight(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                itemModelObject.colorRight = Color.FromHex("#ffce99");
                itemModelObject.colorRightUpdate = Color.FromHex("#ffce99");
                itemModelObject.quantityR += 1;
                totalCount += 1;
                //CartTotal.Text = totalCount.ToString();
                
                if (order.ContainsKey(itemModelObject.itemNameRight))
                {
                    var itemToUpdate = order[itemModelObject.itemNameRight];
                    itemToUpdate.item_quantity = itemModelObject.quantityR;
                    order[itemModelObject.itemNameRight] = itemToUpdate;
                }
                else
                {
                    itemSelected.pur_business_uid = itemModelObject.itm_business_uidRight;
                    itemSelected.item_uid = itemModelObject.item_uidRight;
                    itemSelected.item_name = itemModelObject.itemNameRight;
                    itemSelected.item_quantity = itemModelObject.quantityR;
                    itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceRight.Substring(1).Trim());
                    itemSelected.img = itemModelObject.imageSourceRight;
                    itemSelected.unit = itemModelObject.itemRightUnit;
                    itemSelected.description = itemModelObject.item_descRight;
                    itemSelected.business_price = itemModelObject.item_businessPriceRight;
                    itemSelected.taxable = itemModelObject.itemTaxableRight;
                    itemSelected.isItemAvailable = true;
                    order.Add(itemModelObject.itemNameRight, itemSelected);
                }
                UpdateNumberOfItemsInCart();
            }
        }

        private Dictionary<string, ItemPurchased> purchase = new Dictionary<string, ItemPurchased>();
        void CheckOutClickBusinessPage(System.Object sender, System.EventArgs e)
        {

            purchase = new Dictionary<string, ItemPurchased>();
            foreach (string item in order.Keys)
            {
                if (order[item].item_quantity != 0)
                {
                    if (!selectedDeliveryDate.business_uids.Contains(order[item].pur_business_uid))
                    {
                        order[item].isItemAvailable = false;
                    }
                    else
                    {
                        order[item].isItemAvailable = true;
                    }
                    Debug.WriteLine("ITEM NAME: {0}, IS ITEM AVAILABLE: {1}", item, order[item].isItemAvailable);
                    purchase.Add(item, order[item]);
                }
            }


            Application.Current.Properties["day"] = title.Text;
            Application.Current.MainPage = new CheckoutPage(purchase, selectedDeliveryDate);
            //Application.Current.MainPage = new CartPage(purchase, selectedDeliveryDate);
        }

        void UpdateNumberOfItemsInCart()
        {
            var totalItemsToDelivery = 0;
            foreach (string item in order.Keys)
            {
                if (order[item].item_quantity != 0)
                {
                    if (selectedDeliveryDate.business_uids.Contains(order[item].pur_business_uid))
                    {
                        totalItemsToDelivery += order[item].item_quantity;
                    }
                }
            }
            CartTotal.Text = totalItemsToDelivery.ToString();
        }


        public List<DateTime> GetBusinessSchedule(ServingFreshBusiness data, string businessID)
        {
            var currentDate = DateTime.Now;
            var tempDateTable = GetTable(currentDate);
            var businesDeliverySchedule = new List<DateTime>();
            var businesDisplaySchedule = new List<ScheduleInfo>();

            //Debug.WriteLine("TEMP TABLE FOR LOOK UPS");
            //foreach (DateTime t in tempDateTable)
            //{
            //    Debug.WriteLine(t);
            //}

            foreach (Business a in data.business_details)
            {
                if(businessID == a.z_biz_id)
                {
                    var acceptingDate = LastAcceptingOrdersDate(tempDateTable, a.z_accepting_day, a.z_accepting_time);
                    var deliveryDate = new DateTime();
                    //Debug.WriteLine("CURRENT DATE: " + currentDate);
                    //Debug.WriteLine("LAST ACCEPTING DATE: " + acceptingDate);

                    if (currentDate < acceptingDate)
                    {
                        //Debug.WriteLine("ONTIME");
                        
                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);
                        if(deliveryDate < acceptingDate)
                        {
                            deliveryDate = deliveryDate.AddDays(7);
                        }

                        if (!businesDeliverySchedule.Contains(deliveryDate))
                        {
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = deliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = deliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = deliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = deliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.ToString("ddd") + " " + acceptingDate.ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.WriteLine("business_uids list: ");

                            //foreach (string ID in element.business_uids)
                            //{
                            //    Debug.WriteLine(ID);
                            //}

                            businesDeliverySchedule.Add(deliveryDate);
                            businesDisplaySchedule.Add(element);
                        }
                        else
                        {
                            foreach (ScheduleInfo element in businesDisplaySchedule)
                            {
                                if (element.deliveryTimeStamp == deliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.WriteLine("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.WriteLine(ID);
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("NON ONTIME! -> ROLL OVER TO NEXT DELIVERY DATE");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);

                        if (!businesDeliverySchedule.Contains(deliveryDate.AddDays(7)))
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = nextDeliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = nextDeliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = nextDeliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = nextDeliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.AddDays(7).ToString("ddd") + " " + acceptingDate.AddDays(7).ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.Write("business_uids list: ");

                            //foreach (string ID in element.business_uids)
                            //{
                            //    Debug.Write(ID + ", ");
                            //}

                            businesDeliverySchedule.Add(nextDeliveryDate);
                            businesDisplaySchedule.Add(element);
                        }
                        else
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);

                            foreach (ScheduleInfo element in businesDisplaySchedule)
                            {
                                if (element.deliveryTimeStamp == nextDeliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.Write("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.Write(ID + ", ");
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // DISPLAY SCHEDULE ELEMENTS;
            //Debug.WriteLine("");
            //Debug.WriteLine("");
            //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS NOT SORTED");
            //Debug.WriteLine("");

            //foreach (ScheduleInfo element in businesDisplaySchedule)
            //{
            //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
            //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
            //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
            //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
            //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
            //    Debug.Write("business_uids list: ");

            //    foreach (string ID in element.business_uids)
            //    {
            //        Debug.Write(ID + ", ");
            //    }

            //    Debug.WriteLine("");
            //    Debug.WriteLine("");
            //}

            businesDeliverySchedule.Sort();
            List<ScheduleInfo> sortedSchedule = new List<ScheduleInfo>();

            foreach (DateTime deliveryElement in deliverySchedule)
            {
                foreach (ScheduleInfo scheduleElement in businesDisplaySchedule)
                {
                    if (deliveryElement == scheduleElement.deliveryTimeStamp)
                    {
                        sortedSchedule.Add(scheduleElement);
                    }
                }
            }

            businesDisplaySchedule = sortedSchedule;

            //Debug.WriteLine("");
            //Debug.WriteLine("");
            //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
            //Debug.WriteLine("");

            //foreach (ScheduleInfo element in businesDisplaySchedule)
            //{
            //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
            //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
            //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
            //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
            //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
            //    Debug.Write("business_uids list: ");

            //    foreach (string ID in element.business_uids)
            //    {
            //        Debug.Write(ID + ", ");
            //    }

            //    Debug.WriteLine("");
            //    Debug.WriteLine("");
            //}
            return businesDeliverySchedule;
        }

        public List<DateTime> GetTable(DateTime today)
        {
            var table = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                table.Add(today);
                today = today.AddDays(1);
            }
            return table;
        }

        public DateTime LastAcceptingOrdersDate(List<DateTime> table, string acceptingDay, string acceptingTime)
        {
            var date = DateTime.Parse(acceptingTime);

            foreach(DateTime element in table)
            {
                if (element.DayOfWeek.ToString().ToUpper() == acceptingDay)
                {
                    break;
                }
                date = date.AddDays(1);
            }

            return date;
        }

        public DateTime BussinesDeliveryDate(DateTime lastAcceptingOrdersDate, string day, string time)
        {
            string startTime = "";

            foreach (char a in time.ToCharArray())
            {
                if (a != '-')
                {
                    startTime += a;
                }
                else
                {
                    break;
                }
            }

            var deliveryDate = DateTime.Parse(startTime.Trim());
            //Debug.WriteLine("DELIVERYY DATE IN BUSINESS" + deliveryDate);
            //Debug.WriteLine("LAST ACCEPTING DATE IN BUSINESS" + lastAcceptingOrdersDate);
            if (deliveryDate < lastAcceptingOrdersDate)
            {
                deliveryDate = deliveryDate.AddDays(7);
            }
            //Debug.WriteLine("DEFAULT DELIVERY DATE: " + deliveryDate);

            for (int i = 0; i < 7; i++)
            {
                if (deliveryDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                deliveryDate = deliveryDate.AddDays(1);
            }

            //Debug.WriteLine("DELIVERY DATE: " + deliveryDate);
            return deliveryDate;
        }


        public DateTime lastAcceptingOrdersDate(string day, string hour)
        {
            var acceptingDate = DateTime.Parse(hour);

            //Debug.WriteLine("DEFAULT ACCEPTING ORDERS DATE: " + acceptingDate);
            //Debug.WriteLine("LAST ACCEPTING DAY: " + day.ToUpper());

            for (int i = 0; i < 7; i++)
            {
                if (acceptingDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                acceptingDate = acceptingDate.AddDays(1);
            }

            //Debug.WriteLine("LAST ACCEPTING ORDERS DATE: " + acceptingDate);
            return acceptingDate;
        }

        public DateTime bussinesDeliveryDate(string day, string time)
        {
            string startTime = "";

            foreach (char a in time.ToCharArray())
            {
                if (a != '-')
                {
                    startTime += a;
                }
                else
                {
                    break;
                }
            }

            var deliveryDate = DateTime.Parse(startTime.Trim());
            //Debug.WriteLine("DEFAULT DELIVERY DATE: " + deliveryDate);

            for (int i = 0; i < 7; i++)
            {
                if (deliveryDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                deliveryDate = deliveryDate.AddDays(1);
            }

            //Debug.WriteLine("DELIVERY DATE: " + deliveryDate);
            return deliveryDate;
        }


        void Open_Checkout(Object sender, EventArgs e)
        {

            Application.Current.MainPage = new CheckoutPage(null);
        }
        List<string> filterTypes = new List<string>();
        void Change_Color(Object sender, EventArgs e)
        {

            var myStack = (StackLayout)sender;
            var gestureRecognizer = (TapGestureRecognizer)myStack.GestureRecognizers[0];
            var selectedElement = (Filter)gestureRecognizer.CommandParameter;

            Debug.WriteLine("FILTER LABEL: " + selectedElement.filterName);
            Debug.WriteLine("FILTER SOURCE: " + selectedElement.iconSource);

            if(selectedElement.isSelected == false)
            {
                selectedElement.isSelected = true;
                selectedElement.updateImage = GetImageSouce(selectedElement.filterName);
                if (!filterTypes.Contains(selectedElement.type))
                {
                    filterTypes.Add(selectedElement.type);
                }
            }
            else
            {
                selectedElement.isSelected = false;
                selectedElement.updateImage = SetImageSouce(selectedElement.filterName);
                if (filterTypes.Contains(selectedElement.type))
                {
                    filterTypes.Remove(selectedElement.type);
                }
            }


            if(filterTypes.Count != 0)
            {
                var filteredList = new List<Items>();
                foreach(Items produce in data.result)
                {
                    if (filterTypes.Contains(produce.item_type))
                    {
                        filteredList.Add(produce);
                    }
                }
                GetData(filteredList);

            }
            else
            {
                GetData(data.result);
            }

            foreach (ItemsModel i in datagrid)
            {
                //i.colorLeftUpdate = Color.FromHex("#FFFFFF");
                //i.colorRightUpdate = Color.FromHex("#FFFFFF");
                i.opacityLeftUpdate = 1;
                i.opacityRightUpdate = 1;
                i.isItemLeftEnableUpdate = true;
                i.isItemRightEnableUpdate = true;
                i.isItemLeftUnavailableUpdate = false;
                i.isItemRightUnavailableUpdate = false;

                if (!selectedDeliveryDate.business_uids.Contains(i.itm_business_uidLeft))
                {
                    i.opacityLeftUpdate = 0.5;
                    i.isItemLeftEnableUpdate = false;
                    i.isItemLeftUnavailableUpdate = true;
                }

                if (!selectedDeliveryDate.business_uids.Contains(i.itm_business_uidRight))
                {
                    i.opacityRightUpdate = 0.5;
                    i.isItemRightEnableUpdate = false;
                    i.isItemRightUnavailableUpdate = true;
                }

                if (order.ContainsKey(i.itemNameLeft))
                {
                    i.colorLeftUpdate = Color.FromHex("#ffce99");
                    i.quantityL = order[i.itemNameLeft].item_quantity;
                }

                if (order.ContainsKey(i.itemNameRight))
                {
                    i.colorRightUpdate = Color.FromHex("#ffce99");
                    i.quantityR = order[i.itemNameRight].item_quantity;
                }
            }
            UpdateNumberOfItemsInCart();
        }

        string GetImageSouce(string name)
        {
            var result = "";
            if(name == "Fruits")
            {
                result = "selectedFruitsIcon.png";
            }else if (name == "Vegetables")
            {
                result = "selectedVegetablesIcon.png";
            }else if (name == "Desserts")
            {
                result = "selectedDessertsIcon.png";
            }
            else if (name == "Others")
            {
                result = "selectedOthersIcon.png";
            }
            else if (name == "Favorites")
            {
                result = "selectedFavoritesIcon.png";
            }
            return result;
        }

        string SetImageSouce(string name)
        {
            var result = "";
            if (name == "Fruits")
            {
                result = "unselectedFruitsIcon.png";
            }
            else if (name == "Vegetables")
            {
                result = "unselectedVegetablesIcon.png";
            }
            else if (name == "Desserts")
            {
                result = "unselectedDessertsIcon.png";
            }
            else if (name == "Others")
            {
                result = "unselectedOthersIcon.png";
            }
            else if (name == "Favorites")
            {
                result = "unselectedFavoritesIcon.png";
            }
            return result;
        }


        void Open_Farm(Object sender, EventArgs e)
        {
            var sl = (StackLayout)sender;
            var tgr = (TapGestureRecognizer)sl.GestureRecognizers[0];
            var dm = (ScheduleInfo)tgr.CommandParameter;
            string weekday = dm.delivery_dayofweek;

            selectedDeliveryDate = dm;
            deliveryTime.Text = dm.delivery_time;
            orderBy.Text = "(" + dm.orderExpTime + ")";

            Debug.WriteLine(weekday);
            foreach(string b_uid in dm.business_uids)
            {
                Debug.WriteLine(b_uid);
            }

            foreach(ScheduleInfo i in displaySchedule)
            {
                i.colorScheduleUpdate = Color.FromHex("#E0E6E6");
                i.textColorUpdate = Color.FromHex("#136D74");
            }
            dm.colorScheduleUpdate = Color.FromHex("#FF8500");
            dm.textColorUpdate = Color.FromHex("#FFFFFF");

            Application.Current.Properties["delivery_date"] = dm.delivery_date;
            Application.Current.Properties["delivery_time"] = dm.delivery_time;
            Application.Current.Properties["deliveryDate"] = dm.deliveryTimeStamp;

            foreach (ItemsModel i in datagrid)
            {
                //i.colorLeftUpdate = Color.FromHex("#FFFFFF");
                //i.colorRightUpdate = Color.FromHex("#FFFFFF");
                i.opacityLeftUpdate = 1;
                i.opacityRightUpdate = 1;
                i.isItemLeftEnableUpdate = true;
                i.isItemRightEnableUpdate = true;
                i.isItemLeftUnavailableUpdate = false;
                i.isItemRightUnavailableUpdate = false;

                if (!dm.business_uids.Contains(i.itm_business_uidLeft))
                {
                    i.opacityLeftUpdate = 0.5;
                    i.isItemLeftEnableUpdate = false;
                    i.isItemLeftUnavailableUpdate = true;
                }
                else if (!dm.business_uids.Contains(i.itm_business_uidRight))
                {
                    i.opacityRightUpdate = 0.5;
                    i.isItemRightEnableUpdate = false;
                    i.isItemRightUnavailableUpdate = true;
                }
            }
            UpdateNumberOfItemsInCart();
            //ItemsPage businessItemPage = new ItemsPage(types, dm.business_uids, weekday);
            //Application.Current.MainPage = businessItemPage;
        }



        void Change_Border_Color(Object sender, EventArgs e)
        {
            
            var f = (Frame)sender;
            var tgr = (TapGestureRecognizer)f.GestureRecognizers[0];
            var bc = (BusinessCard)tgr.CommandParameter;
            if (bc.border_color == Color.LightGray)
            {
                business.Clear();
                business.Add(selectedBusiness(new Business()
                {
                    business_image = bc.business_image,
                    business_name = bc.business_name,
                    z_biz_id = bc.business_uid
                }));
                var businessDeliveryDates = GetBusinessSchedule(data, bc.business_uid);
                List<ScheduleInfo> businessDisplaySchedule = new List<ScheduleInfo>();

                foreach(DateTime deliveryElement in businessDeliveryDates)
                {
                    foreach(ScheduleInfo scheduleElement in displaySchedule)
                    {
                        if(deliveryElement == scheduleElement.deliveryTimeStamp)
                        {
                            businessDisplaySchedule.Add(scheduleElement);
                            break;
                        }
                    }
                }

                //Debug.WriteLine("");
                //Debug.WriteLine("");
                //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
                //Debug.WriteLine("");

                //foreach (ScheduleInfo element in businessDisplaySchedule)
                //{
                //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                //    Debug.Write("business_uids list: ");

                //    foreach (string ID in element.business_uids)
                //    {
                //        Debug.Write(ID + ", ");
                //    }

                //    Debug.WriteLine("");
                //    Debug.WriteLine("");
                //}

                //farm_list.ItemsSource = business;
                delivery_list.ItemsSource = businessDisplaySchedule;
            }
            else
            {
                //farm_list.ItemsSource = businesses;
                delivery_list.ItemsSource = displaySchedule;
            }

        }

        void CheckOutClickDeliveryDaysPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage(null);
        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            // SHOULDN'T MOVE SINCE YOU ARE IN THIS PAGE
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }

        void InfoClick(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Application.Current.Properties["guest"])
            {
                Application.Current.MainPage = new InfoPage();
            }
            
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Application.Current.Properties["guest"])
            {
                Application.Current.MainPage = new ProfilePage();
            }
        }

        void ToScheduleView(System.Object sender, System.EventArgs e)
        {
            var initialWidth = new GridLength(0);

            if (deliveryDatesColumn.Width.Equals(initialWidth))
            {
                deliveryDatesColumn.Width = columnWidth;
                filtersColumn.Width = 0;
            }
        }

        void ToFiltersView(System.Object sender, System.EventArgs e)
        {
            var initialWidth = new GridLength(0);

            if (filtersColumn.Width.Equals(initialWidth))
            {
                deliveryDatesColumn.Width = 0;
                filtersColumn.Width = columnWidth;
            }
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            var selectedImage = (ImageButton)sender;
            var pickedElement = (ItemsModel)selectedImage.CommandParameter;

            pickedElement.favoriteIconLeftUpdate = "selectedFavoritesIcon.png";
        }
    }
}
