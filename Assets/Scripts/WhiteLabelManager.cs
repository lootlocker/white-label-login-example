using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using LootLocker.Requests;

public class WhiteLabelManager : MonoBehaviour
{
    // Input fields
    [Header("New User")]
    public TMP_InputField newUserEmailInputField;
    public TMP_InputField newUserPasswordInputField;
    public TMP_InputField nickNameInputField;

    [Header("Existing User")]
    public TMP_InputField existingUserEmailInputField;
    public TMP_InputField existingUserPasswordInputField;
    public CanvasAnimator loginCanvasAnimator;

    [Header("Reset password")]
    public TMP_InputField resetPasswordInputField;

    [Header("RememberMe")]
    // Components for enabling auto login
    public Toggle rememberMeToggle;
    public Animator rememberMeAnimator;
    private int rememberMe;

    [Header("Button animators")]
    public Animator autoLoginButtonAnimator;
    public Animator loginButtonAnimator;
    public Animator backButtonAnimator;
    public Animator loginBackButtonAnimator;
    public Animator newUserButtonAnimator;
    public Animator resetPasswordButtonAnimator;

    [Header("Player name")]
    public TextMeshProUGUI playerNameText;
    public Animator playerNameTextAnimator;

    // Called when pressing "LOGIN" on the login-page
    public void Login()
    {
        string email = existingUserEmailInputField.text;
        string password = existingUserPasswordInputField.text;
        LootLockerSDKManager.WhiteLabelLogin(email, password, Convert.ToBoolean(rememberMe), response =>
        {
            if (!response.success)
            {
                // Error
                // Animate the buttons
                loginButtonAnimator.SetTrigger("Error");
                backButtonAnimator.SetTrigger("Show");
                Debug.Log("error while logging in");
                return;
            }
            else
            {
                Debug.Log("Player was logged in succesfully");
            }

            // Is the account verified?
            if (response.VerifiedAt == null)
            {
                // Stop here if you want to require your players to verify their email before continuing
            }

            LootLockerSDKManager.StartWhiteLabelSession((response) =>
            {
                if (!response.success)
                {
                    // Error
                    // Animate the buttons
                    loginButtonAnimator.SetTrigger("Error");
                    backButtonAnimator.SetTrigger("Show");
                    Debug.Log("error starting LootLocker session");
                    return;
                }
                else
                {
                    // Session was succesfully started;
                    // animate the buttons
                    loginButtonAnimator.SetTrigger("LoggedIn");
                    backButtonAnimator.SetTrigger("Show");
                    Debug.Log("session started successfully");
                    // Write the current players name to the screen
                    SetPlayerNameToGameScreen();
                }
            });
        });
    }

    // Write the players name to the screen
    void SetPlayerNameToGameScreen()
    {
        LootLockerSDKManager.GetPlayerName((response) =>
        {
            if (response.success)
            {
                playerNameTextAnimator.ResetTrigger("Hide");
                playerNameTextAnimator.SetTrigger("Show");
                playerNameText.text = response.name;
            }
        });
    }

    // Called when pressing "CREATE" on new user screen
    public void NewUser()
    {
        string email = newUserEmailInputField.text;
        string password = newUserPasswordInputField.text;
        string newNickName = nickNameInputField.text;

        // Local function for errors
        void Error(string error)
        {
            Debug.Log(error);
            newUserButtonAnimator.SetTrigger("Error");
            backButtonAnimator.SetTrigger("Show");
        }

        LootLockerSDKManager.WhiteLabelSignUp(email, password, (response) =>
        {
            if (!response.success)
            {
                Error(response.Error);
                return;
            }
            else
            {
                // Succesful response
                // Log in player to set name
                // Login the player
                LootLockerSDKManager.WhiteLabelLogin(email, password, false, response =>
                {
                    if (!response.success)
                    {
                        Error(response.Error);
                        return;
                    }
                    // Start session
                    LootLockerSDKManager.StartWhiteLabelSession((response) =>
                    {
                        if (!response.success)
                        {
                            Error(response.Error);
                            return;
                        }
                        // Set nickname to be public UID if nothing was provided
                        if (newNickName == "")
                        {
                            newNickName = response.public_uid;
                        }
                        // Set new nickname for player
                        LootLockerSDKManager.SetPlayerName(newNickName, (response) =>
                        {
                            if (!response.success)
                            {
                                Error(response.Error);
                                return;
                            }

                            // End this session
                            LootLockerSessionRequest sessionRequest = new LootLockerSessionRequest();
                            LootLocker.LootLockerAPIManager.EndSession(sessionRequest, (response) =>
                            {
                                if (!response.success)
                                {
                                    Error(response.Error);
                                    return;
                                }
                                Debug.Log("Account Created");
                                newUserButtonAnimator.SetTrigger("AccountCreated");
                                backButtonAnimator.SetTrigger("Show");
                                // New user, turn off remember me
                                rememberMeToggle.isOn = false;
                            });
                        });
                    });
                });
            }
        });
    }

    // Start is called before the first frame update
    public void Start()
    {
        // See if we should log in the player automatically
        rememberMe = PlayerPrefs.GetInt("rememberMe", 0);
        if (rememberMe == 0)
        {
            rememberMeToggle.isOn = false;
        }
        else
        {
            rememberMeToggle.isOn = true;
        }
    }

    // Called when changing the value on the toggle
    public void ToggleRememberMe()
    {
        bool rememberMeBool = rememberMeToggle.isOn;
        rememberMe = Convert.ToInt32(rememberMeBool);

        // Animate button
        if (rememberMeBool == true)
        {
            rememberMeAnimator.SetTrigger("On");
        }
        else
        {
            rememberMeAnimator.SetTrigger("Off");
        }
        PlayerPrefs.SetInt("rememberMe", rememberMe);
    }

    public void AutoLogin()
    {
        // Does the user want to automatically log in?
        if (Convert.ToBoolean(rememberMe) == true)
        {
            // Hide the buttons on the login screen
            existingUserEmailInputField.GetComponent<Animator>().ResetTrigger("Show");
            existingUserEmailInputField.GetComponent<Animator>().SetTrigger("Hide");
            existingUserEmailInputField.GetComponent<Animator>().ResetTrigger("Show");
            existingUserPasswordInputField.GetComponent<Animator>().SetTrigger("Hide");
            loginBackButtonAnimator.ResetTrigger("Show");
            loginBackButtonAnimator.SetTrigger("Hide");

            // Start to spin the login button
            loginButtonAnimator.ResetTrigger("Hide");
            loginButtonAnimator.SetTrigger("Show");
            loginButtonAnimator.SetTrigger("Login");

            LootLockerSDKManager.CheckWhiteLabelSession(response =>
            {
                if (response == false)
                {
                    // Session was not valid, show error animation
                    // and show back button
                    loginButtonAnimator.SetTrigger("Error");
                    backButtonAnimator.SetTrigger("Show");

                    // set the remember me bool to false here, so that the next time the player press login
                    // they will get to the login screen
                    rememberMeToggle.isOn = false;
                }
                else
                {
                    // Session is valid, start game session
                    LootLockerSDKManager.StartWhiteLabelSession((response) =>
                    {
                        if (response.success)
                        {
                            // It was succeful, log in
                            loginButtonAnimator.SetTrigger("LoggedIn");
                            backButtonAnimator.SetTrigger("Show");
                            // Write the current players name to the screen
                            SetPlayerNameToGameScreen();
                        }
                        else
                        {
                            // Error
                            // Animate the buttons
                            loginButtonAnimator.SetTrigger("Error");
                            backButtonAnimator.SetTrigger("Show");

                            Debug.Log("error starting LootLocker session");
                            // set the remember me bool to false here, so that the next time the player press login
                            // they will get to the login screen
                            rememberMeToggle.isOn = false;

                            return;
                        }

                    });

                }

            });
        }
        else if(Convert.ToBoolean(rememberMe) == false)
        {
            // Continue as usual
            loginCanvasAnimator.CallAppearOnAllAnimators();
        }
    }

    public void PasswordReset()
    {
        string email = resetPasswordInputField.text;
        LootLockerSDKManager.WhiteLabelRequestPassword(email, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("error requesting password reset");
                resetPasswordButtonAnimator.SetTrigger("Error");
                backButtonAnimator.SetTrigger("Show");
                return;
            }

            Debug.Log("requested password reset successfully");
            resetPasswordButtonAnimator.SetTrigger("Done");
            backButtonAnimator.SetTrigger("Show");
        });
    }

    public void ResendVerificationEmail()
    {
        int playerID = 0;
        LootLockerSDKManager.WhiteLabelRequestVerification(playerID, (response) =>
        {
            if (response.success)
            {
                // Email was sent!
            }
        });
    }
}
