using Assets.SignalRModels;
using Assets.SIgnalRServices;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInputHandlerScript : MonoBehaviour
{
    [SerializeField]
    private InputField _playerName;
    [SerializeField]
    private InputField _wordKeyToCreateRoom;
    [SerializeField]
    private InputField _wordKeyToJoinRoom;
    [SerializeField]
    private InputField _playerNumber;

    [SerializeField]
    private Text _roomCreationResult;
    private string _wordKey;

    [SerializeField]
    private GameObject _roomJoiningForCreator;
    [SerializeField]
    private GameObject _loader;
    [SerializeField]
    private GameObject _waitingScreen;

    private string _roomCreationRoute = "http://localhost:5000/api/Home/create-room";
    private string _playerCreationRoute = "http://localhost:5000/api/Home/create-player";

    public void ConvertCaseToUpper(InputField inputField)
    {
        inputField.text = inputField.text.ToUpper();
    }

    public void GiveRoomCreationResult()
    {
        //string welcome = $"Welcome, {_playerName.text}!";
        //string roomCreationResult = "Room has been created succesfully.";
        //_welcomingText.text = (welcome + '\n' + '\n' + roomCreationResult).ToUpper();
        _roomCreationResult.text = "Room has been created succesfully.".ToUpper();

        _loader.SetActive(false);
        _roomJoiningForCreator.SetActive(true);
    }

    public void GivePlayerCreationResult()
    {
        _loader.SetActive(false);
        _waitingScreen.SetActive(true);

    }

    public void SendRoomCreationRequest()
    {
        StartCoroutine(SendRoomCreationData());
        Debug.Log("Start Coroutine \"Room creation\"");
    }

    public void SendPlayerCreationRequest()
    {
        StartCoroutine(SendPlayerCreationData());
        Debug.Log("Start Coroutine \"Player creation\"");
    }

    private IEnumerator SendRoomCreationData()
    {
        string jsonRequest = $"{{\"WordKey\": \"{_wordKeyToCreateRoom.text}\", \"PlayerNumber\": {_playerNumber.text}}}";
        yield return CreateRequest(_roomCreationRoute, jsonRequest);

        GiveRoomCreationResult();
    }

    private IEnumerator SendPlayerCreationData()
    {

        if (_wordKeyToCreateRoom.text != "")
        {
            _wordKey = _wordKeyToCreateRoom.text;
        }
        else
        {
            _wordKey = _wordKeyToJoinRoom.text;
        }

        string jsonRequest = $"{{\"PlayerName\": \"{_playerName.text}\", \"WordKey\": \"{_wordKey}\"}}";
        yield return CreateRequest(_playerCreationRoute, jsonRequest, (requestResponse) =>
        {
            GameDataStorage.CurrentClient = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientDataModel>(requestResponse);
        });

        GivePlayerCreationResult();
    }

    private IEnumerator CreateRequest(string route, string jsonRequest, Action<string> OnRequestDone = null)
    {
        UnityWebRequest request = new UnityWebRequest(route, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("An error has uccured: " + request.error);
            yield break;
        }

        OnRequestDone?.Invoke(request.downloadHandler.text);

        Debug.Log("Server response: " + request);
        Debug.Log("Server response: " + request.result);
        Debug.Log("Status code: " + request.responseCode);
    }
}
