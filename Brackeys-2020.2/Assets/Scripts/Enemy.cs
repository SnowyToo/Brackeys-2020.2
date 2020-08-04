﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject deathParticles;
    [SerializeField]
    private GameObject greenDeath;
    [SerializeField]
    private AudioClip[] deathSounds;

    private Animator anim;

    [SerializeField]
    private int collisionDamage;

    [HideInInspector]
    public EnemySpawner.EnemyType type;

    // Attributes
    public enum AttributeType { SHIELD, TIME_ONLY, REFLECT }
    [HideInInspector]
    public List<EnemyAttribute> attributes = new List<EnemyAttribute>();
    [HideInInspector]
    public Stack<Shield> shields = new Stack<Shield>();
    [SerializeField]
    private GameObject shield;
    [SerializeField]
    private GameObject reflect;
    [SerializeField]
    protected GameObject warpSprite; //I will keep using time warp out of spite now.
    protected GameObject curSprite;

    [HideInInspector]
    public bool invincible;

    protected readonly EnemyAttribute TIME_WARP = new EnemyAttribute(AttributeType.TIME_ONLY);

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Debug.Log("MY TYPE IS: " + type);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Photon"))
        {
            PhotonHit();
        }
        else if (other.CompareTag("Player"))
        {
            PlayerHit();
        }
        else if (other.CompareTag("Laser"))
        {
            LaserHit();
        }
    }

    public virtual void PhotonHit()
    {
        if (invincible) return;

        if (attributes.Contains(TIME_WARP) && !GameManager.isRewinding)
        {
            if(curSprite == null)
                curSprite = Instantiate(warpSprite, transform.position, Quaternion.identity, transform);
            return;
        }

        if(shields.Count > 0) return;

        Die();
    }

    public virtual void PlayerHit()
    {
        GameManager.player.TakeDamage(collisionDamage);
    }

    public virtual void LaserHit() { }

    public void ShieldHit(bool hurt)
    {
        GameManager.CameraShake(0.2f, 0.2f);
        if (hurt)
        {
            shields.Pop();
            anim.SetTrigger("Hit");
            StartCoroutine(Invincibility());
        }
    }

    public IEnumerator Invincibility()
    {
        invincible = true;
        yield return new WaitForSeconds(1.75f);
        invincible = false;
    }

    public void Die(bool remove = true, bool collectPoints = true)
    {
        if (remove) GameManager.enemySpawner.RemoveEnemy(type);
        if (collectPoints) GameManager.EnemyKill(this);
        float volume = 0.8f;
        if (gameObject.tag == "LaserEnemy")
            volume = 0.4f;

        GameManager.PlaySound(deathSounds, this.gameObject, volume);

        GameObject particles = deathParticles;
        if (attributes.Contains(TIME_WARP))
            particles = greenDeath;

        GameManager.SpawnParticles(particles, gameObject);
        GameManager.CameraShake(0.2f, 0.3f);
        Destroy(gameObject);
    }

    public void Addtribute(EnemyAttribute attribute)
    {
        if (attribute.type == AttributeType.SHIELD && attribute.amount == 0) return;

        attributes.Add(attribute);

        if (attribute.type == AttributeType.SHIELD)
        {
            for(int i = 0; i < attribute.amount; i++)
            {
                Shield s = Instantiate(shield, transform.position, Quaternion.identity, transform).GetComponent<Shield>();

                s.transform.localScale = (1.6f + 0.3f * i) * new Vector2(1, 1);
                s.transform.localEulerAngles = Vector3.forward * 20f * i;
                
                s.parent = this;
                shields.Push(s);
            }
        }

        if(attribute.type == AttributeType.TIME_ONLY)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        }

        if(attribute.type == AttributeType.REFLECT)
        {
            Shield s = Instantiate(reflect, transform.position, Quaternion.identity, transform).GetComponent<Shield>();
            s.transform.localScale = 1.9f * new Vector2(1, 1);

            s.parent = this;
        }
    }
}

[System.Serializable]
public struct EnemyAttribute
{
    public Enemy.AttributeType type;

    public int amount;

    public EnemyAttribute(Enemy.AttributeType _type, int _amount = 0)
    {
        type = _type;
        amount = _amount;
    }

    public bool Equals(EnemyAttribute other)
    {
        return type == other.type;
    }
}
