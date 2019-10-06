using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using Oruga.Types;
using Firebase;
using Firebase.Database;
using Firebase.Functions;
using Firebase.Unity.Editor;
using Firebase.Storage;
using Firebase.Auth;

using Artie;
using System.Runtime.InteropServices;
using System.Threading;

namespace Oruga.Services
{
    [CreateAssetMenu]
    public class FirebaseService : ScriptableObject
    {
        public BoolVariable debug;
        public BoolVariable localServices;

        private DatabaseReference _dbReference;
        private FirebaseFunctions _functions;
        private FirebaseStorage _storage;
        
        private FirebaseAuth _auth;
        private FirebaseUser _user;
        private PhoneAuthProvider _phoneProvider;
        public uint phoneVerificationTimeoutMS = 90000;
        private string _smsVerificationId;
        private ForceResendingToken _smsResendToken;
        private string _smsVerificationIdTimeoutRequest;
        private Credential _userCredential;
        
        public class ChangeOnUserStateEvent : UnityEvent<FirebaseUser> {}
        public ChangeOnUserStateEvent changeOnUserState = new ChangeOnUserStateEvent();
        public UserService userService;
        
        private void OnEnable()
        {
            bool isEditor = false;

#if UNITY_EDITOR
            isEditor = true;
#endif

            if (isEditor)
            {
                FirebaseApp firebaseApp = FirebaseApp.Create(
                    FirebaseApp.DefaultInstance.Options,
                    "FIREBASE_EDITOR");

                firebaseApp.SetEditorDatabaseUrl("https://artie-data.firebaseio.com/");

                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    if (task.Result == DependencyStatus.Available)
                    {
                        _dbReference = FirebaseDatabase.GetInstance(firebaseApp).RootReference;
                        _storage = FirebaseStorage.GetInstance(firebaseApp);
                        _auth = FirebaseAuth.GetAuth(firebaseApp);
                        _phoneProvider = PhoneAuthProvider.GetInstance(_auth);
                        _functions = FirebaseFunctions.DefaultInstance;
                        if (localServices.Value)
                            _functions.UseFunctionsEmulator("http://localhost:5001");
                        
                        // Listener for authentications changes
                        _auth.StateChanged += this.AuthStateChanged;
                        AuthStateChanged(this, null);
                    }
                    else
                    {
                        Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
                        Console.WriteLine("Could not resolve all Firebase dependencies: " + task.Result);
                        
                        // Listener for authentications changes
                        _auth.StateChanged += this.AuthStateChanged;
                        AuthStateChanged(this, null);
                    }
                });
            }
            else
            {
                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://artie-data.firebaseio.com/");
                _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                _storage = FirebaseStorage.DefaultInstance;
                _functions = Firebase.Functions.FirebaseFunctions.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _phoneProvider = PhoneAuthProvider.GetInstance(_auth);
                
                // Listener for authentications changes
                _auth.StateChanged += this.AuthStateChanged;
                AuthStateChanged(this, null);
            }
        }

        public string GetUserId()
        {
            return _user?.UserId;
        }
        
        /// <summary>
        /// Executes a remote function and return async result
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<HttpsCallableResult> CallRemoteFunction(string functionName, Dictionary<string, object> data)
        {
            var function = _functions.GetHttpsCallable(functionName);
            
            // Because context on firebase functions is not populated on all cases -.-
            if (_auth != null)
            {
                data["uid"] = _user.UserId;
            }
            return function.CallAsync(data);
        }

        /// <summary>
        /// Get an instant snapshot from realtime db in firebase
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<DataSnapshot> RequestDbSnapshot(string path)
        {
            return _dbReference.Database.GetReference(path).GetValueAsync();
        }

        /// <summary>
        /// Get file from firebase storage on a binary collection fashion
        /// </summary>
        /// <param name="fileReference"></param>
        /// <returns>Async task that contains byte string representation of file</returns>
        public Task<byte[]> GetFileReference(string fileReference)
        {
            return _storage.GetReference(fileReference)
                .GetBytesAsync(1024 * 1024)
                .ContinueWith((Task<byte[]> task) =>
                {
                    if (!task.IsFaulted) return !task.IsCompleted ? null : task.Result;
                    return null;

                });

        }

        /// <summary>
        /// Creates a NEW user inside firebase platform using email and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>Async task that contains just created user</returns>
        public async Task<FirebaseUser> SignupUserWithEmailAndPassword(string email, string password)
        {
            var userCreationTask = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            try
            {
                var newUser = await userCreationTask;
                if (debug.Value)
                {
                    Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
                }

                return newUser;
            }
            catch (Exception e)
            {
                if (userCreationTask.IsCanceled)
                {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    Console.WriteLine("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return null;
                }

                if (userCreationTask.IsFaulted)
                {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + userCreationTask.Exception);
                    Console.WriteLine("CreateUserWithEmailAndPasswordAsync encountered an error: " + userCreationTask.Exception);
                    return null;
                }
                
                Debug.LogError(e);
                Console.WriteLine(e);
                return null;
            }
        }
        
        /// <summary>
        /// Signin existing user with email and password  
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>Async task that contains just created user</returns>
        public async Task<FirebaseUser> SigninWithEmailAndPassword(string email, string password)
        {
            var signinUserTask = _auth.SignInWithEmailAndPasswordAsync(email, password);
            try
            {
                var user = await signinUserTask;
                if (debug.Value)
                {
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        user.DisplayName, user.UserId);
                }

                return user;
            }
            catch (Exception e)
            {
                if (signinUserTask.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    Console.WriteLine("SignInWithEmailAndPasswordAsync was canceled.");
                    return null;
                }

                if (signinUserTask.IsFaulted)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + signinUserTask.Exception);
                    Console.WriteLine("SignInWithEmailAndPasswordAsync encountered an error: " + signinUserTask.Exception);
                    return null;
                }
                
                Debug.LogError(e);
                Console.WriteLine(e);
                return null;
            }
        }
        
        public async Task<bool> SendResetPasswordEmail(string email)
        {
            var sendResetMailTask = _auth.SendPasswordResetEmailAsync(email);
            try
            {
                await sendResetMailTask;
                return true;
            }
            catch (Exception e)
            {
                if (sendResetMailTask.IsCanceled)
                {
                    Debug.LogError("Password reset was canceled.");
                    Console.WriteLine("Password reset was canceled.");
                    return false;
                }

                if (sendResetMailTask.IsFaulted)
                {
                    Debug.LogError("Password reset encountered an error.");
                    Console.WriteLine("Password reset encountered an error.");
                    return false;
                }
                
                Debug.LogError(e);
                Console.WriteLine(e);
                return false;
            }
        }

        public void VerifyPhoneNumber(string phoneNumber)
        {
            _phoneProvider.VerifyPhoneNumber(phoneNumber, phoneVerificationTimeoutMS, null,
                verificationCompleted: (credential) => {
                    Debug.Log("verificationCompleted");
                    // Auto-sms-retrieval or instant validation has succeeded (Android only).
                    // There is no need to input the verification code.
                    // `credential` can be used instead of calling GetCredential().
                    _userCredential = credential;
                },
                verificationFailed: (error) => {
                    Debug.Log("verificationFailed");
                    // The verification code was not sent.
                    // `error` contains a human readable explanation of the problem.
                },
                codeSent: (id, token) => {
                    Debug.Log("codeSent");
                    // Verification code was successfully sent via SMS.
                    // `id` contains the verification id that will need to passed in with
                    // the code from the user when calling GetCredential().
                    // `token` can be used if the user requests the code be sent again, to
                    // tie the two requests together.
                    _smsVerificationId = id;
                    _smsResendToken = token;
                }, 
                codeAutoRetrievalTimeOut: (id) =>
                {
                    Debug.Log("codeAutoRetrievalTimeOut");
                    _smsVerificationIdTimeoutRequest = id;
                    ReSendPhoneNumberVerification(phoneNumber);
                }
            );
        }
        
        public void ReSendPhoneNumberVerification(string phoneNumber)
        {
            _phoneProvider.VerifyPhoneNumber(phoneNumber, phoneVerificationTimeoutMS, _smsResendToken,
                verificationCompleted: (credential) => {
                    Debug.Log("verificationCompleted");
                    // Auto-sms-retrieval or instant validation has succeeded (Android only).
                    // There is no need to input the verification code.
                    // `credential` can be used instead of calling GetCredential().
                    _userCredential = credential;
                },
                verificationFailed: (error) => {
                    Debug.Log("verificationFailed");
                    // The verification code was not sent.
                    // `error` contains a human readable explanation of the problem.
                },
                codeSent: (id, token) => {
                    Debug.Log("codeSent");
                    // Verification code was successfully sent via SMS.
                    // `id` contains the verification id that will need to passed in with
                    // the code from the user when calling GetCredential().
                    // `token` can be used if the user requests the code be sent again, to
                    // tie the two requests together.
                    _smsVerificationId = id;
                    _smsResendToken = token;
                }, 
                codeAutoRetrievalTimeOut: (id) =>
                {
                    Debug.Log("codeAutoRetrievalTimeOut");
                    _smsVerificationIdTimeoutRequest = id;
                }
            );
        }

        public async Task<FirebaseUser> AuthenticateUserWithSMSVerificationCode(string verificationCode)
        {
            var credential = _phoneProvider.GetCredential(_smsVerificationId, verificationCode);
            var credentialTask = _auth.SignInWithCredentialAsync(credential);
            try
            {
                var user = await credentialTask;
                if (debug.Value)
                {
                    Debug.LogFormat("User signed in successfully: {0} ({1} {2} {3})",
                        user.DisplayName, user.UserId, user.PhoneNumber, user.ProviderId);
                }

                return user;
            }
            catch (Exception e)
            {
                if (credentialTask.IsCanceled)
                {
                    Debug.LogError("AuthenticateUserWithSMSVerificationCode was canceled.");
                    Console.WriteLine("AuthenticateUserWithSMSVerificationCode was canceled.");
                    return null;
                }

                if (credentialTask.IsFaulted)
                {
                    Debug.LogError($"AuthenticateUserWithSMSVerificationCode encountered an error: {credentialTask.Exception}");
                    Console.WriteLine($"AuthenticateUserWithSMSVerificationCode encountered an error: {credentialTask.Exception}");
                    return null;
                }
                
                Debug.LogError(e);
                Console.WriteLine(e);
                return null;
            }
        }

        public void Singout()
        {
            _auth.SignOut();
        }

        /// <summary>
        ///  Catch changes on user state authentication. Emits event with latest user state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (_auth.CurrentUser != _user) {
                var signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;
                if (!signedIn && _user != null && debug.Value) {
                    Debug.Log("Signed out " + _user.UserId);
                }
                _user = _auth.CurrentUser;
                if (signedIn && debug.Value)
                {
                    Debug.Log("Signed in " + _user.UserId);
                    Debug.Log("displayName " + _user.DisplayName ?? "");
                    Debug.Log("emailAddress " + _user.Email ?? ""); 
                    Debug.Log("photoUrl " + _user.PhotoUrl ?? "");
                    Debug.Log("phoneNumber " + _user.PhoneNumber ?? "");
                    Debug.Log("ProviderId " + _user.ProviderId ?? "");
                }
            }
            else
            {
                _user = _auth.CurrentUser;
                if (debug.Value)
                {
                    Debug.Log("User re-authenticated?");
                }
            }

            if (userService != null)
            {
                userService.ChangeOnUserState(_user);
            }
        }
    }
}
