using UnityEngine;

public class DetectCollsion : MonoBehaviour
{
    private NetworkPlayer networkPlayer;
    private Rigidbody hitRigidbody;

    private ContactPoint[] contactPoints = new ContactPoint[5];

    private void Awake()
    {
        networkPlayer = GetComponentInParent<NetworkPlayer>();
        hitRigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���û�У�clentʲô����ִ��
        if (!networkPlayer.HasStateAuthority) return;

        if (!networkPlayer.IsActiveRagdoll) return;

        if (!collision.collider.CompareTag("CauseDamage")) return;
        
        // ��ֹ�Լ������������Լ�
        if(collision.collider.transform.root == networkPlayer.transform) return;

        int numberOfContacts = collision.GetContacts(contactPoints);

        for(int i = 0; i < numberOfContacts; i++)
        {
            ContactPoint contactPoint = contactPoints[i];

            // ��ȡcontact impulse
            Vector3 contactImpulse = contactPoint.impulse / Time.deltaTime;

            // �жϵ�ǰ�����Ƿ��㹻��
            if (contactImpulse.magnitude < 15) continue;

            networkPlayer.OnPlayerBodyPartHit();

            Vector3 forceDirection = (contactImpulse + Vector3.up) * 0.5f;

            forceDirection = Vector3.ClampMagnitude(forceDirection, 30);

            Debug.DrawRay(hitRigidbody.position, forceDirection * 40, Color.red, 4);

            hitRigidbody.AddForce(forceDirection, ForceMode.Impulse);
        }
    }
}