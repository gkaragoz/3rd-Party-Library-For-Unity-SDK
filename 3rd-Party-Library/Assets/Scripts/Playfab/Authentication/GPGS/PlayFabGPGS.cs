﻿using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;
using PlayFab.DataModels;

public class PlayFabGPGS : MonoBehaviour
{
    public Text text;

    private void Awake()
    {
        // The following grants profile access to the Google Play Games SDK.
        // Note: If you also want to capture the player's Google email, be sure to add
        // .RequestEmail() to the PlayGamesClientConfiguration
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestEmail()
        .RequestIdToken()
        .RequestServerAuthCode(false)
        .Build();
        
        PlayGamesPlatform.InitializeInstance(config);

        // Recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;

        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();

        //Remember User
        RememberGoogleAccount();
    }

    #region AUTHENTICATE

    public void LoginPlayGameService(bool linkAction)
    {
        //Authenticate the user request
        Social.localUser.Authenticate((bool success) => {

            if (success) // If "Auth" is success
            {   
                //Get "ServerAuthCode" and assing it.
                string serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();

                Debug.Log("Server Auth Code: " + serverAuthCode);

                text.text = GetUserName();

                if (linkAction) // is GPGS Linking Action ?
                {
                    LinkWithGooglePlayAccount(serverAuthCode);
                }

                else
                {
                    //Login in with PlayFab
                    LoginWithGoogleAccout(serverAuthCode);
                }
                
            }

            else
            {    
                Debug.Log("Google Failed to Authorize your login");
            }

        });
    }

    //GPGS login data with PlayFab Integration.
    public void LoginWithGoogleAccout(string authCode)
    {
        PlayerPrefs.SetString("GPGSAUTH", "success"); // PlayFab GPGS Auth succeed.

        //PlayFab Google Account Integration
        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
        {
            TitleId = PlayFabSettings.TitleId,

            ServerAuthCode = authCode,

            CreateAccount = true

        }, (result) =>

        {
            Debug.Log("Signed In as " + result.PlayFabId);

            string entityID = result.EntityToken.Entity.Id;

            string entityType = result.EntityToken.Entity.Type;

            /********************************************************************************************/
                                               /*CLOUD SAVE*/

            //Determinete the cloud Service Type in { "FILE", "STATISTIC", "OBJECT" }
            string cloudMethodType = (CSPlayFabMaster.cloudType.DATATABLE)
                                      .ToString();

            //CloudSave Instance
            CSPlayFabMaster cloud = new CSPlayFabMaster(entityID, entityType, cloudMethodType);

            /********************************************************************************************/

        }, 
        
        OnPlayFabError); // Error Callback
        
    }

    //Error Callback
    private void OnPlayFabError(PlayFabError error)
    {
        PlayerPrefs.SetString("GPGSAUTH", "failed"); // PlayFab GPGS Auth failed.

        Debug.LogError("GPGS PlayFab - Error Report: " + error.GenerateErrorReport());
    }

    #endregion

    #region RECOVER

    //Remember User whose signed in with Google Acc.
    private void RememberGoogleAccount()
    {
        if (PlayerPrefs.HasKey("GPGSAUTH"))
        {
            if (PlayerPrefs.GetString("GPGSAUTH").Equals("success"))
            {
                LoginPlayGameService(false); // Login Google Acc. automaticly
            }

        }

    }

    #endregion

    #region LINK

    //Link user account with GPGS
    private void LinkWithGooglePlayAccount(string authCode)
    {
        PlayFabClientAPI.LinkGoogleAccount(new LinkGoogleAccountRequest()
        {
            ServerAuthCode = authCode

        }, (result) =>

        {
            Debug.Log("Account Linked With Google Play Succeed.");

        },

        OnPlayFabError); // Error Callback

    }

    #endregion

    //Return user Display Name
    public string GetUserName()
    {
        return PlayGamesPlatform.Instance.GetUserDisplayName();
    }

}
