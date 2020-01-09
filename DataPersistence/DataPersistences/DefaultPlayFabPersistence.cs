namespace d4160.DataPersistence
{
    using System;
    using PlayFab.ClientModels;
    using PlayFab;
    using UnityEngine.GameFoundation.DataPersistence;
    using System.Collections.Generic;

    public class DefaultPlayFabPersistence : RemoteDataPersistence
    {
        protected bool m_encrypted = false;
        protected IStorageHelper m_storageHelper = null;

        public DefaultPlayFabPersistence(
            IDataSerializer serializer,
            bool encrypted,
            string authenticationId,
            IStorageHelper storageHelper = null) : base(serializer, authenticationId)
        {
            m_encrypted = encrypted;
            m_storageHelper = storageHelper;
        }

        public override void Load<T>(string identifier, Action<T> onLoadCompleted = null, Action<Exception> onLoadFailed = null)
        {
            if (m_storageHelper == null)
            {
                PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
                    PlayFabId = AuthenticationId,
                    Keys = null
                }, result => {
                    //Debug.Log("Got user data:");
                    if (result.Data == null || !result.Data.ContainsKey(identifier))
                    {
                        onLoadFailed?.Invoke(null);
                    }
                    else
                    {
                        var data = DeserializeString<T>(result.Data[identifier].Value);
                        onLoadCompleted?.Invoke(data);
                    }
                }, (error) => {
                    //Debug.Log("Got error retrieving user data:");
                    //Debug.Log(error.GenerateErrorReport());

                    onLoadFailed?.Invoke(null);
                });
            }
            else
            {
                m_storageHelper.Load(m_encrypted,
                    () =>{
                        onLoadCompleted?.Invoke((T)m_storageHelper);
                    }
                );
            }
        }

        public override void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null)
        {
            if (m_storageHelper == null)
            {
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
                    Data = new Dictionary<string, string>() {
                        { identifier, SerializeString(content) }
                    }
                },
                result => {
                    //Debug.Log("Successfully updated user data");
                    onSaveCompleted?.Invoke();
                },
                error => {
                    //Debug.Log("Got error setting user data Ancestor to Arthur");
                    //Debug.Log(error.GenerateErrorReport());

                    onSaveFailed?.Invoke(null);
                });
            }
            else
            {
                var storageHelper = content as IStorageHelper;
                storageHelper.StorageHelperType = m_storageHelper.StorageHelperType;

                if (storageHelper != null)
                {
                    storageHelper.Save(m_encrypted,
                        () => {
                            onSaveCompleted?.Invoke();
                        });
                }
                else
                {
                    onSaveFailed?.Invoke(null);
                }
            }
        }

        protected string SerializeString(object o)
        {
            return serializer.Serialize(o, m_encrypted);
        }

        protected T DeserializeString<T>(string value) where T : ISerializableData
        {
            return serializer.Deserialize<T>(value, m_encrypted);
        }
    }
}