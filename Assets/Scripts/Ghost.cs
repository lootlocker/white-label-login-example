using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ghost : MonoBehaviour
{
    public RectTransform parentRect;

    private Image parentImage;

    public int numberOfGhosts;

    private List<RectTransform> ghosts = new List<RectTransform>();

    public List<Vector2> sizes = new List<Vector2>();

    public List<Quaternion> rotations = new List<Quaternion>();

    public List<Color> colors = new List<Color>();

    public List<Image> images = new List<Image>();

    public GameObject ghostPrefab;

    private Vector4 colorVector = new Vector4(1f, 1f, 1f, 1f);

    public float ghostIncrementalAlpha = 0.7f;

    public bool off;

    // Start is called before the first frame update
    void Start()
    {
        parentImage = parentRect.GetComponent<Image>();
        CreateGhosts();
    }

    void CreateGhosts()
    {
        for (int i = 0; i < numberOfGhosts; i++)
        {
            GameObject newGhost = Instantiate(ghostPrefab, transform.parent);
            newGhost.transform.position = parentRect.position;
            newGhost.transform.SetAsFirstSibling();
            ghosts.Add(newGhost.GetComponent<RectTransform>());
            sizes.Add(Vector2.zero);
            rotations.Add(Quaternion.identity);
            colors.Add(Color.black);
            images.Add(newGhost.GetComponent<Image>());
            images[i].raycastTarget = false;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(off)
        {
            for (int i = 0; i < numberOfGhosts; i++)
            {
                images[i].color = Vector4.zero;
            }
            return;
        }
        for (int i = 0; i < numberOfGhosts; i++)
        {
            if(i == 0)
            {
                ghosts[i].sizeDelta = sizes[i];
                ghosts[i].localRotation = rotations[i];

                colorVector = parentImage.color;
                colorVector.w = ghostIncrementalAlpha;
                images[i].color = colorVector;

                sizes[i] = parentRect.sizeDelta;
                rotations[i] = parentRect.localRotation;
            }
            else
            {
                ghosts[i].sizeDelta = sizes[i];
                ghosts[i].localRotation = rotations[i];

                colorVector = images[i - 1].color;
                colorVector.w *= ghostIncrementalAlpha;
                images[i].color = colorVector;

                sizes[i] = ghosts[i-1].sizeDelta;
                rotations[i] = ghosts[i - 1].localRotation;
            }
        }
    }
}
