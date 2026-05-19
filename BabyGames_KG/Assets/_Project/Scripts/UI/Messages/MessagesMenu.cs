using DataClasses;
using Helpers;
using Loggers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Message;
using UnityEngine;

namespace UI
{
    public class MessagesMenu : MonoBehaviour, ISelfValidator
    {
        public static MessagesMenu instance { get; private set; }

        [SerializeField] private List<KeyedObject<string, BasicMessage>> _messages = new();
        [SerializeField] private List<BasicMessage> _spawnedMessages = new List<BasicMessage>();

        public void Validate(SelfValidationResult result)
        {
            foreach (var item in _messages)
            {
                item.key = item.value.name;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        public async void ShowMessage(string id)
        {
            await ShowMessageAsync(id);
        }

        public async Task ShowMessageAsync(string id)
        {
            if (_spawnedMessages.Exists(x => x != null && x.name.Contains(id))) { return; }

            if (Screen.orientation == ScreenOrientation.Portrait) { id += "-tall"; }
            else { id += "-wide"; }

            try
            {
                var reference = _messages.Find(x => x.key == id);
                if (reference == null) { return; }

                var message = Instantiate(reference.value, Vector3.zero, Quaternion.identity, transform);
                if (message == null) { return; }

                _spawnedMessages.Add(message);
                message.name = id;

                message.Show(() =>
                {
                    _spawnedMessages.Remove(message);
                    Destroy(message?.gameObject);
                });

                await AsyncHelper.NextFrame();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error showing message");
            }
            finally
            {
                _spawnedMessages.RemoveNulls();
            }
        }

        public void DeleteAll()
        {
            try
            {
                foreach (var item in _spawnedMessages)
                {
                    _spawnedMessages.Remove(item);
                    Destroy(item?.gameObject);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            _spawnedMessages.RemoveNulls();
        }
    }
}