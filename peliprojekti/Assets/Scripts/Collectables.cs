using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    public enum CollectibleType { PowerUp, BalloonPowerUp, LifeUp, LevelCollectible, Points };
    public CollectibleType collectibleType;
    [Space(10)]

    public int points;

    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider2D;

    [Header("PowerUp")]
    public SpriteRenderer effectSpriteRenderer;

    [Header("Balloon")]
    public bool goToBonusLevel;
    public Transform bonusLevelTargetPosition;

    [Header("Level Collectibles")]
    public int levelNumber;
    public int levelCollectibleNumber;
    public bool bonusLevelCollectible;  //  go back to gamelevel
    private int collected;

    private Vector2 startPosition;


    public void Start()
    {
        startPosition = transform.position;

        if (collectibleType == CollectibleType.LevelCollectible)
        {
            //Change Color
            collected = PlayerPrefs.GetInt("Collectible_" + levelNumber + "_" + levelCollectibleNumber);
            if (collected == 1) { spriteRenderer.color = Color.green; }
            else { spriteRenderer.color = Color.white; }
        }
    }

    public void Collect()
    {
        PlayerPrefs.SetInt("Collectible_" + levelNumber + "_" + levelCollectibleNumber, 1);  //  Collectable_1_1,   0 - 1
        PlayerPrefs.Save();

        collected = 1;
        Disable();
    }

    public void Disable()
    {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;

        if (effectSpriteRenderer) { effectSpriteRenderer.enabled = false; }
        if (collectibleType == CollectibleType.BalloonPowerUp) 
        {
            Transform resetParent = GameObject.Find("GameManager").transform;
            transform.SetParent(resetParent);
        }
    }

    public void Reset()
    {
        if (collectibleType == CollectibleType.LevelCollectible)
        {
            //Change Color
            collected = PlayerPrefs.GetInt("Collectible_" + levelNumber + "_" + levelCollectibleNumber);
            if (collected == 1) {   spriteRenderer.color = Color.green;   }   
            else {   spriteRenderer.color = Color.white;   }
        }

        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;

        if (effectSpriteRenderer) { effectSpriteRenderer.enabled = true; }

        if (collectibleType == CollectibleType.BalloonPowerUp)
        {
            transform.parent = null;
            transform.position = startPosition;
            transform.GetComponent<BoxCollider2D>().enabled = true;
        } 
    }
}