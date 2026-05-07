using UnityEngine;
using Platformer.Mechanics;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public PlayerController playerController;

    void Update()
    {

        UpdateFirePointPosition();
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("bulletPrefab が設定されていません");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("firePoint が設定されていません");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("playerController が設定されていません");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript == null)
        {
            Debug.LogError("Bullet Prefab に Bullet.cs が付いていません");
            return;
        }

        Debug.Log("発射時のfacingDirection: " + playerController.facingDirection);

        bulletScript.direction = playerController.facingDirection;
    }

    void UpdateFirePointPosition()
    {
        Vector3 pos = firePoint.localPosition;
        pos.x = Mathf.Abs(pos.x) * playerController.facingDirection;
        firePoint.localPosition = pos;
    }
}