using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Information")]
    [Tooltip("Name of weapon")]
    [SerializeField]
    private string weaponName;
    [Tooltip("Max ammo")]
    [SerializeField]
    private int maxAmmo = 8;
    [Tooltip("Bullet per shot")]
    [SerializeField]
    private int bulltersPerShot = 1;
    [Tooltip("Bullet max range")]
    [SerializeField]
    private float range = 100f;
    [Tooltip("Bullet damage")]
    [SerializeField]
    private int bulletDamage = 10;
    private int m_CurrentAmmo;
    private float m_LastTimeShot;
    private int totalBullets;

    [Header("References")]
    [Tooltip("Tip of the weapon")]
    [SerializeField]
    public GameObject shotObject;
    [Tooltip("Ignore physics layer")]
    [SerializeField]
    private LayerMask layerMask;
    [Tooltip("Impact effect")]
    [SerializeField]
    private GameObject impactEffect;
    [Tooltip("Destroy impact effect after X seconds")]
    [SerializeField]
    private float impactSeconds = 2f;
    [Tooltip("shotObject effect (GameObject)")]
    [SerializeField]
    private GameObject shotObjectEffectPrefab;

    // Start is called before the first frame update
    void Start()
    {
        m_CurrentAmmo = maxAmmo;
        totalBullets = maxAmmo;
    }

    public void ShowWeapon(bool show)
    {
        gameObject.SetActive(show);
    }

    public void UseAmmo(int amount)
    {
        m_CurrentAmmo -= amount;
        if (m_CurrentAmmo < 0) m_CurrentAmmo = 0;
        m_LastTimeShot = Time.time;
    }

    private bool CanShot()
    {
        //return (m_CurrentAmmo - bulltersPerShot) > 0;
        return true;
    }

    public void HandleShoot()
    {
        if (CanShot()) {
            UseAmmo(bulltersPerShot);

            ShotEffect();
            RaycastHit hit;
            if (Physics.Raycast(shotObject.transform.position, shotObject.transform.forward, out hit, range)) {
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    // TODO: add "splash" sfx
                    // TODO: add "splash" particle effect
                    target.TakeDamage(bulletDamage);
                }

                if (impactEffect != null) {
                    GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, impactSeconds);
                }
            }
        }
    }

    private void ShotEffect()
    {
        Instantiate(
            shotObjectEffectPrefab, shotObject.transform);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
