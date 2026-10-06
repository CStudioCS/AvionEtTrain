using UnityEngine;

public class Shoot : Ability
{
    private GameObject bullet;
    private Avion avion;
    private float bulletSpeed;

    public Shoot(float cooldown, float bulletSpeed, Avion avion, GameObject bullet) : base(cooldown)
    {
        this.avion = avion;
        this.bulletSpeed = bulletSpeed;
        this.bullet = bullet;
    }

    protected override void UseAbility()
    {
        GameObject thisBullet = GameObject.Instantiate(bullet,avion.transform.position , avion.transform.rotation);
        Bullet thisBulletScript = thisBullet.GetComponent<Bullet>();
        thisBulletScript.dir = avion.movementInput;
        thisBulletScript.speed = bulletSpeed;

        
    }


}

