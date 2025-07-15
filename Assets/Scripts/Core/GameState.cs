using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Enum representing all possible game states
    /// </summary>
    public enum GameState
    {
        Menu,
        Setup,
        Playing,
        Paused,
        GameOver,
        Victory
    }
    
    /// <summary>
    /// Enum representing different turn phases
    /// </summary>
    public enum TurnPhase
    {
        StartTurn,
        AttackPhase,
        DefensePhase,
        EndTurn
    }
    
    /// <summary>
    /// Enum representing card suits
    /// </summary>
    public enum CardSuit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }
    
    /// <summary>
    /// Enum representing player types
    /// </summary>
    public enum PlayerType
    {
        Human,
        AI
    }
    
    /// <summary>
    /// Enum representing different card ability types
    /// </summary>
    public enum CardAbilityType
    {
        None,
        Shield,
        DoubleTrouble,
        TrumpChanger,
        Blocker,
        Magnet,
        Reverser,
        SkipTurn,
        ExtraDraw,
        Wildcard,
        DoubleDefense
    }
}