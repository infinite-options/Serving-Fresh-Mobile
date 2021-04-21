﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Essentials;
using Acr.UserDialogs;
using System.Text;
using static ServingFresh.Views.SelectionPage;

namespace ServingFresh.Views
{
    public partial class RefundPage : ContentPage
    {
        Stream photoStream = null;

        public RefundPage()
        {
            InitializeComponent();
            refundItemImage.Source = "refund.png";
            refundItemImage.Scale = 0.7;
            CartTotal.Text = CheckoutPage.total_qty.ToString();
            refundEmail.Text = (string)Application.Current.Properties["user_email"];
        }

        async void TakePictureClick(System.Object sender, System.EventArgs e)
        {
            try
            {
                var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { SaveToAlbum = true });

                if (photo != null)
                {
                    //Get the public album path
                    var aPpath = photo.AlbumPath;

                    //Get private path
                    var path = photo.Path;
                    photoStream = photo.GetStream();
                    refundItemImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                    refundItemImage.Scale = 1;
                }
            }
            catch
            {
                await DisplayAlert("Permission required", "We'll need permission to access your camara, so that you can take a photo of the damaged product.", "OK");
                return;
            }
        }

        async void ChoosePictureFromGalleryClick(System.Object sender, System.EventArgs e)
        {
            try
            {
                var photo = await Plugin.Media.CrossMedia.Current.PickPhotoAsync();

                if (photo != null)
                {
                    photoStream = photo.GetStream();
                    refundItemImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                    refundItemImage.Scale = 1;
                }
            }
            catch
            {
                await DisplayAlert("Permission required", "We'll need permission to access your camara roll, so that you can select a photo of the damaged product.", "OK");
                return;
            }
        }

        async void SendRefundRequest(System.Object sender, System.EventArgs e)
        {
            if (photoStream == null)
            {
                await DisplayAlert("Missing photo", "Please take a photo of your damage product with the button below", "OK");
                return;
            }
            if (refundEmail.Text == null)
            {
                await DisplayAlert("Email can't be empty", "Please fill in your email", "OK");
                return;
            }

            if (refundNote.Text == null)
            {
                await DisplayAlert("Message can't be empty", "Please fill in the message", "OK");
                return;
            }
            try
            {

                var userEmail = (string)Application.Current.Properties["user_email"];
                // var userMessage = message.Text;
                // var userPhone = "4158329643";
                // var userImage = PhotoImage.Source;

                HttpClient client = new HttpClient();
                MultipartFormDataContent content = new MultipartFormDataContent();
                StringContent userEmailContent = new StringContent(userEmail.ToLower(), Encoding.UTF8);
                // StringContent userPhoneContent = new StringContent(refundPhone.Text.ToLower().Trim(), Encoding.UTF8);
                StringContent userMessageContent = new StringContent(refundNote.Text.ToLower().Trim(), Encoding.UTF8);

                var ms = new MemoryStream();
                photoStream.CopyTo(ms);
                byte[] TargetImageByte = ms.ToArray();
                ByteArrayContent userImageContent = new ByteArrayContent(TargetImageByte);

                // content.Add(userEmailContent, "client_email");
                // content.Add(userPhoneContent, "client_phone");
                // content.Add(userMessageContent, "client_message");

                content.Add(userEmailContent, "email");
                content.Add(userMessageContent, "note");
                //content.Add(userMessageContent, "client_message");

                // CONTENT, NAME, FILENAME
                content.Add(userImageContent, "item_photo", "product_image.png");

                var request = new HttpRequestMessage();
                // request.RequestUri = new Uri("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/refundDetailsNEW");
                request.RequestUri = new Uri("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/Refund");
                request.Method = HttpMethod.Post;
                request.Content = content;

                UserDialogs.Instance.ShowLoading("Sending your request...");
                HttpResponseMessage response = await client.SendAsync(request);
                Debug.WriteLine("This is the response from request.isSuccess" + response.IsSuccessStatusCode);
                Debug.WriteLine("This is the response from request" + response.Content.ReadAsStringAsync());
                UserDialogs.Instance.HideLoading();
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Request Sent!", "Give us a day or two to respond.", "OK");
                    //await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Fail!", "Sorry! Something went wrong", "OK");
                    //await Navigation.PopAsync();
                }
                return;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception Caught: " + exception.ToString());
                return;
            }
        }

        void NavigateToCartFromRefunds(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }

        void NavigateToHistoryFromRefunds(System.Object sender, System.EventArgs e)
        {
            NavigateToHistory(sender, e);
        }
    }
}
