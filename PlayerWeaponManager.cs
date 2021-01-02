using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerController))]
public class PlayerWeaponManager : MonoBehaviour
{
    [Tooltip("List of weapons")]
    [SerializeField]
    private List<WeaponController> listWeapons = new List<WeaponController>();

    [Header("References")]
    [SerializeField]
    private Camera weaponCamera;

    private PlayerInputHandler m_InputHandler;
    private PlayerController m_PlayerController;
    private WeaponController m_CurrentWeapon;
    // Start is called before the first frame update
    void Start()
    {
        m_InputHandler = GetComponent<PlayerInputHandler>();
        m_PlayerController = GetComponent<PlayerController>();

        if (listWeapons.Count > 0) {
            m_CurrentWeapon = listWeapons[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_InputHandler.GetFireInputDown() && m_CurrentWeapon) {
            m_CurrentWeapon.HandleShoot();
        }
    }
}
