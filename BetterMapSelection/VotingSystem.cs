using System;
using UnityEngine;
using CementTools;
using CementTools.Modules.InputModule;
using System.Collections.Generic;
using Femur;
using Random = UnityEngine.Random;

public class VotingSystem
{
    public Dictionary<Actor, Vector2> _actorPositions = new Dictionary<Actor, Vector2>();
    public Dictionary<Actor, ActorGraphic> _actorGraphics = new Dictionary<Actor, ActorGraphic>();
    private bool _busy = true;
    int numberOfRows;
    int numberOfCollumns;
    int mapBitsLength;
    public MapUIBit[] mapBits;
    Transform canvasParent;
    float timer;
    float timeWaited;
    bool movedThisFrame;

    public event Action<int> VotingEnded;

    public VotingSystem(float time, MapUIBit[] mapBits, int numberOfCollumns, Transform canvasParent)
    {
        Cement.Log($"Creating voting system! {mapBits.Length}");
        mapBitsLength = mapBits.Length;
        this.mapBits = mapBits;
        this.numberOfCollumns = numberOfCollumns;
        numberOfRows = Mathf.CeilToInt(mapBitsLength / numberOfCollumns);
        this.canvasParent = canvasParent;

        timer = time;

        BindKeys();
    }

    private void BindKeys()
    {
        InputManager.onInput(Input.d).bind(dPressed);
        InputManager.onInput(Input.leftstickRight).bind(dPressed);

        InputManager.onInput(Input.a).bind(aPressed);
        InputManager.onInput(Input.leftstickLeft).bind(aPressed);

        InputManager.onInput(Input.w).bind(wPressed);
        InputManager.onInput(Input.leftstickUp).bind(wPressed);

        InputManager.onInput(Input.s).bind(sPressed);
        InputManager.onInput(Input.leftstickDown).bind(sPressed);
    }

    private void UnbindKeys()
    {
        InputManager.onInput(Input.d).unbind(dPressed);
        InputManager.onInput(Input.leftstickRight).unbind(dPressed);

        InputManager.onInput(Input.a).unbind(aPressed);
        InputManager.onInput(Input.leftstickLeft).unbind(aPressed);

        InputManager.onInput(Input.w).unbind(wPressed);
        InputManager.onInput(Input.leftstickUp).unbind(wPressed);

        InputManager.onInput(Input.s).unbind(sPressed);
        InputManager.onInput(Input.leftstickDown).unbind(sPressed);
    }

    private bool IsPositionValid(Vector2 vec)
    {
        if (vec.x < 0 || vec.y < 0)
        {
            return false;
        }
        if (vec.x >= numberOfCollumns || vec.y > numberOfRows)
        {
            return false;
        }
        if (vec.x + vec.y * numberOfCollumns > mapBitsLength - 1)
        {
            return false;
        }
        return true;
    }

    private void dPressed(Actor a)
    {
        if (!_busy)
            return;

        Cement.Log("PRESSED D");

        MoveActor(a, Vector2.right);
    }

    private void aPressed(Actor a)
    {
        if (!_busy)
            return;

        Cement.Log("PRESSED A");

        MoveActor(a, Vector2.left);
    }

    private void wPressed(Actor a)
    {
        if (!_busy)
            return;

        Cement.Log("PRESSED W");

        MoveActor(a, Vector2.down);
    }

    private void sPressed(Actor a)
    {
        if (!_busy)
            return;

        Cement.Log("PRESSED S");

        MoveActor(a, Vector2.up);
    }

    private void AddActor(Actor a)
    {
        _actorPositions[a] = Vector2.zero;
        GameObject actorGraphic = GameObject.Instantiate(BMSResources.actorGraphic);
        actorGraphic.transform.SetParent(canvasParent);
        _actorGraphics[a] = actorGraphic.AddComponent<ActorGraphic>();
        _actorGraphics[a].SetStickerColour(a.primaryColor);
        actorGraphic.transform.eulerAngles = new Vector3(0, -90, 0);
        actorGraphic.transform.localScale = Vector3.one;
    }

    private void MoveActor(Actor a, Vector2 direction)
    {
        if (!_actorPositions.ContainsKey(a))
        {
            AddActor(a);
            UpdateGraphic(a, _actorPositions[a]);
            return;
        }

        Vector2 desiredPosition = _actorPositions[a] + direction;

        if (IsPositionValid(desiredPosition))
        {
            movedThisFrame = true;
            _actorPositions[a] = desiredPosition;
            UpdateGraphic(a, desiredPosition);
        }
    }

    private void UpdateGraphic(Actor a, Vector2 pos)
    {
        int graphicIndex = (int)pos.x + (int)pos.y * numberOfCollumns;
        _actorGraphics[a].transform.position = mapBits[graphicIndex].transform.position;
        _actorGraphics[a].UpdateSticker();
    }

    public void Tick(float deltaTime)
    {
        if (!_busy)
        {
            return;
        }

        if (_actorPositions.Count == 0)
        {
            return;
        }

        if (movedThisFrame)
        {
            timeWaited = 0;
        }
        else
        {
            timeWaited += deltaTime;
        }
        
        if (timer <= 0 || timeWaited > 2f)
        {
            EndVote();
            if (VotingEnded != null)
            {
                VotingEnded.Invoke(GetResult());
            }
        }
        movedThisFrame = false;
    }

    public void EndVote()
    {
        _busy = false;
        foreach (ActorGraphic graphic in _actorGraphics.Values)
        {
            GameObject.Destroy(graphic.gameObject);
        }
        UnbindKeys();
    }

    private int GetResult()
    {
        if (_actorPositions.Count == 0)
        {
            return Random.Range(0, mapBitsLength - 1);
        }

        Dictionary<int, int> _indexOccurrences = new Dictionary<int, int>();
        foreach (Vector2 vec in _actorPositions.Values)
        {
            int index = (int)vec.x + (int)vec.y * numberOfCollumns;
            if (!_indexOccurrences.ContainsKey(index))
            {
                _indexOccurrences[index] = 0;
            }
            _indexOccurrences[index]++;
        }

        int mostAbundant = -1;
        List<int> mostAbundantIndices = new List<int>();

        foreach (int index in _indexOccurrences.Keys)
        {
            if (_indexOccurrences[index] > mostAbundant)
            {
                mostAbundantIndices = new List<int>();
                mostAbundantIndices.Add(index);
                mostAbundant = _indexOccurrences[index];
            }
            else if (_indexOccurrences[index] == mostAbundant)
            {
                mostAbundantIndices.Add(index);
            }
        }

        return mostAbundantIndices[Random.Range(0, mostAbundantIndices.Count - 1)];
    }
}