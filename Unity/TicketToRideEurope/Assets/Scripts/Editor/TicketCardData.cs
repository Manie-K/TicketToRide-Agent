using UnityEngine;

[CreateAssetMenu(fileName = "NewTicketCard", menuName = "TicketToRide/Ticket Card Data")]
public class TicketCardData : ScriptableObject
{
    public string city1;
    public string city2;
    public int points;
}