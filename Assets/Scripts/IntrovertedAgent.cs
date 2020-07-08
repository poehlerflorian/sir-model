using UnityEngine;

namespace Assets.Scripts {
    public class IntrovertedAgent : CitizenAgent {
        protected override void OnCollisionEnter(Collision collision) {
            ICitizen citizenAgent = collision.GetContact(0).otherCollider.GetComponent<ICitizen>();
            if (citizenAgent == null) {
                return;
            }

            AddReward(-0.05f);

            base.OnCollisionEnter(collision);
        }
    }
}
