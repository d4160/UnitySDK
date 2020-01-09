namespace d4160.DataPersistence
{
    using System;
    using PlayFab;
    using PlayFab.ClientModels;

    public class CustomIdAuthenticator : IAuthenticator
    {
        protected string m_playfabId;
        protected string m_customId;
        protected Action<LoginResult> m_loginResultCallback;
        protected Action<PlayFabError> m_loginErrorCallback;

        public string Id => m_playfabId;

        public CustomIdAuthenticator(string customId,
            Action<LoginResult> loginResultCallback,
            Action<PlayFabError> loginErrorCallback)
        {
            m_customId = customId;
            m_loginResultCallback = loginResultCallback;
            m_loginErrorCallback = loginErrorCallback;
        }

        public virtual void Login()
        {
            var request = new LoginWithCustomIDRequest { CustomId = m_customId, CreateAccount = true };
            PlayFabClientAPI.LoginWithCustomID(
                request,
                (result) => {
                    OnLoginSuccess(result);
                    m_loginResultCallback?.Invoke(result);
                },
                (error) => {
                    OnLoginFailure(error);
                    m_loginErrorCallback?.Invoke(error);
                }
            );
        }

        public virtual void Logout()
        {
            m_playfabId = string.Empty;

            m_loginResultCallback?.Invoke(null);
        }

        protected virtual void OnLoginSuccess(LoginResult result)
        {
            m_playfabId = result.PlayFabId;
        }

        protected virtual void OnLoginFailure(PlayFabError error)
        {
        }
    }
}