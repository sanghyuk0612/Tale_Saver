using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
public class Item : MonoBehaviour
{
    public int scoreValue; // 각 아이템의 점수

    void  OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.Score+=scoreValue; // 아이템 점수 추가
            Destroy(gameObject); // 아이템 제거
        }
    }
}