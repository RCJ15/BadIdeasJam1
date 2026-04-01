using System.Collections.Generic;
using UnityEngine;

[SingletonMode(true)]
public class PlayerDeck : Singleton<PlayerDeck>
{
    public static List<Command> Deck => _deck;
    private static List<Command> _deck = null;

    [SerializeField] private CommandReference[] startingDeck;

    protected override void Awake()
    {
        base.Awake();

        _deck = new();

        foreach (Command command in startingDeck)
        {
            _deck.Add(command);
        }
    }

    private void Update()
    {
        
    }
}
