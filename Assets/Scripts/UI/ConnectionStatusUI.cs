using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ConnectionStatusUI : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Format")] 
    [SerializeField] private string connectingMsg = "Conectando…";
    [SerializeField] private string joiningMsgFmt = "Entrando sala… {0:F1}/{1:F0}s";
    [SerializeField] private string creatingMsg = "Creando sala…";
    [SerializeField] private bool showRoomName = false; // si es true muestra el nombre, si no, solo jugadores

    private float animT;

    void Awake()
    {
        if (statusText == null)
        {
            statusText = GetComponent<TextMeshProUGUI>();
        }
        UpdateImmediate();
    }

    void Update()
    {
        animT += Time.deltaTime;
        if (statusText == null) return;

        if (!PhotonNetwork.IsConnected)
        {
            statusText.text = connectingMsg + Dots();
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            int count = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : PhotonNetwork.PlayerList.Length;
            if (showRoomName && PhotonNetwork.CurrentRoom != null)
            {
                statusText.text = $"Sala: {PhotonNetwork.CurrentRoom.Name} ({count})";
            }
            else
            {
                statusText.text = count == 1 ? "Jugadores: 1" : $"Jugadores: {count}";
            }
            return;
        }

        var nm = NetworkManager.Instance;
        if (nm != null && !nm.IsInRoom())
        {
            float t = nm.CurrentJoinTimer;
            float to = nm.JoinRoomTimeout;
            if (t < to)
            {
                statusText.text = string.Format(joiningMsgFmt, t, to);
            }
            else
            {
                statusText.text = creatingMsg + Dots();
            }
        }
    }

    string Dots()
    {
        int dots = (int)(animT * 3f) % 4;
        return new string('.', dots);
    }

    void UpdateImmediate()
    {
        if (statusText == null) return;
        if (!PhotonNetwork.IsConnected)
        {
            statusText.text = connectingMsg;
        }
        else if (PhotonNetwork.InRoom)
        {
            int count = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : PhotonNetwork.PlayerList.Length;
            statusText.text = count == 1 ? "Jugadores: 1" : $"Jugadores: {count}";
        }
        else
        {
            statusText.text = joiningMsgFmt;
        }
    }

    public override void OnConnectedToMaster() { UpdateImmediate(); }
    public override void OnJoinedRoom() { UpdateImmediate(); }
    public override void OnJoinRoomFailed(short returnCode, string message)
    { if (statusText != null) statusText.text = creatingMsg + " (" + message + ")"; }
    public override void OnCreateRoomFailed(short returnCode, string message)
    { if (statusText != null) statusText.text = "Create failed: " + message + Dots(); }
    public override void OnDisconnected(DisconnectCause cause)
    { if (statusText != null) statusText.text = "Desconectado: " + cause; }
    public override void OnPlayerEnteredRoom(Player newPlayer) { UpdateImmediate(); }
}
