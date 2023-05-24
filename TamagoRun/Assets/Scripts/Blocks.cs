using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    public enum BlockType { BasicBreakable, PowerUp };
    public BlockType blockType;

    public SpriteRenderer spriteRenderer;
    public BoxCollider2D[] boxColliders;

    public void Disable()
    {
        if (blockType == BlockType.BasicBreakable)
        {
            //  Sprite Renderer Disable
            spriteRenderer.enabled = false;
            //  Box Colliders 2D - Disable 3x
            foreach (BoxCollider2D b in boxColliders)
            {
                b.enabled = false;
            }
        }
    }

    public void Reset()
    {
        if (blockType == BlockType.BasicBreakable)
        {
            //  Sprite Renderer Enable
            spriteRenderer.enabled = true;
            //  Box Colliders 2D - Enable 3x
            foreach (BoxCollider2D b in boxColliders)
            {
                b.enabled = true;
            }
        }
    }
}
