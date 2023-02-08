using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Firebase.Auth;
using System.Reactive;

namespace ProjApp.Services
{
    public interface IAuthService
    {
        IObservable<Unit> SignAnonymously();
        IObservable<Unit> SignInWithEmailAndPassword(string email, string password);
        IObservable<Unit> SendSignInLink(string toEmail);
        IObservable<Unit> SignInWithEmailLink(string email, string link);
        Task<IFirebaseUser> SignInWithGoogle();
        IObservable<Unit> SignInWithFacebook();
        IObservable<Unit> SignInWithApple();
        IObservable<Unit> VerifyPhoneNumber(string phoneNumber);
        IObservable<Unit> SignInWithPhoneNumberVerificationCode(string verificationCode);
        IObservable<Unit> LinkWithEmailAndPassword(string email, string password);
        Task<IFirebaseUser> LinkWithGoogle();
        IObservable<Unit> LinkWithFacebook();
        IObservable<Unit> LinkWithPhoneNumberVerificationCode(string verificationCode);
        IObservable<Unit> UnlinkProvider(string providerId);
        IObservable<Unit> SignOut();
        IObservable<string[]> FetchSignInMethods(string email);
        IObservable<Unit> SendPasswordResetEmail();

        bool IsSignInWithEmailLink(string link);
        IFirebaseUser CurrentUser { get; }

        IObservable<IFirebaseUser> CurrentUserTicks { get; }
        IObservable<bool> IsSignedInTicks { get; }
        IObservable<bool> IsSignInRunningTicks { get; }
    }
}
