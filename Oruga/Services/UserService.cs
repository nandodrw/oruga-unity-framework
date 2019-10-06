using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Oruga.Models;
using Oruga.Types;
using UnityEngine;
using UnityEngine.Events;

namespace Oruga.Services
{
    [CreateAssetMenu]
    public class UserService : ScriptableObject
    {
        public FirebaseService fbService;
        
        private User _user;
        
        // Variables for SingUp Flow
        public StringVariable userEmail;
        public StringVariable password;
        public StringVariable phoneCountryCode;
        public StringVariable phoneNumber;
        public StringVariable smsVerificationCode;
        public UnityEvent signupValid;

        public UIManager uiManager;
        
        public void ChangeOnUserState(FirebaseUser fbUser)
        {
            if (fbUser != null)
            {
                _user = new User
                {
                    id = fbUser.UserId,
                    email = fbUser.Email,
                    phoneNumber = fbUser.PhoneNumber
                };
                signupValid.Invoke();
                if (uiManager != null)
                {
                    uiManager.ShowControlsUI();
                }
            }
            else
            {
                Debug.Log("User signed out");
                _user = null;
            }
        }
        
        public async Task<bool> SignupByEmailAndPassword()
        {
            var fbUser = await fbService.SignupUserWithEmailAndPassword(userEmail.Value, password.Value);
            if (fbUser != null) return true;
            Debug.LogError("Error creating user");
            return false;
        }

        public async Task<bool> SendResetPasswordEmail()
        {
            return await fbService.SendResetPasswordEmail(userEmail.Value);
        }
        
        public async Task<bool> SigninByEmailAndPassword()
        {
            var fbUser = await fbService.SigninWithEmailAndPassword(userEmail.Value, password.Value);
            if (fbUser != null) return true;
            Debug.LogError("Error signin user");
            return false;
        }

        public void GetSMSVerificationCode()
        {
            fbService.VerifyPhoneNumber($"+{phoneCountryCode}{phoneNumber}");
        }

        public async Task<bool> AuthenticateWithSMSVerificationCode()
        {
            var fbUser = await  fbService.AuthenticateUserWithSMSVerificationCode(smsVerificationCode.Value);
            if (fbUser != null) return true;
            Debug.LogError("Verifying SMS");
            return false;
        }
        
        public void ReSendSMSVerificationCode()
        {
            fbService.ReSendPhoneNumberVerification($"+{phoneCountryCode}{phoneNumber}");
        }

        public void SingOutUser()
        {
            fbService.Singout();
        }
    }
    
}
