using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static Camera mainCamera;

    /// <summary>
    /// Gets the mouse position in world coordinates.
    /// </summary>
    /// <returns>The mouse position in world coordinates.</returns>
    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 mouseScreenPosition = Input.mousePosition;

        // Clamp mouse position to screen size
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        worldPosition.z = 0f;

        return worldPosition;
    }

    /// <summary>
    /// Gets the angle in degrees from a vector.
    /// </summary>
    /// <param name="vector">The vector to get the angle from.</param>
    /// <returns>The angle in degrees.</returns>
    public static float GetAngleFromVector(Vector3 vector)
    {
        float radians = Mathf.Atan2(vector.y, vector.x);

        float degrees = radians * Mathf.Rad2Deg;

        return degrees;
    }

    /// <summary>
    /// Gets the direction vector from an angle in degrees.
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>
    /// <returns>The direction vector.</returns>
    public static Vector3 GetDirectionVectorFromAngle(float angle)
    {
        Vector3 directionVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0f);
        return directionVector;
    }

    /// <summary>
    /// Gets the aim direction based on the angle in degrees.
    /// </summary>
    /// <param name="angleDegrees">The angle in degrees.</param>
    /// <returns>The aim direction.</returns>
    public static AimDirection GetAimDirection(float angleDegrees)
    {
        if (angleDegrees > 22f && angleDegrees <= 67f)
        {
            return AimDirection.UpRight;
        }
        if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            return AimDirection.Up;
        }
        if (angleDegrees > 112f && angleDegrees <= 158f)
        {
            return AimDirection.UpLeft;
        }
        if ((angleDegrees > 158f && angleDegrees <= 180f) || (angleDegrees > -180f && angleDegrees <= -135f))
        {
            return AimDirection.Left;
        }
        if (angleDegrees > -135f && angleDegrees <= -45f)
        {
            return AimDirection.Down;
        }
        if ((angleDegrees > -45f && angleDegrees <= 0f) || (angleDegrees > 0f && angleDegrees <= 22f))
        {
            return AimDirection.Right;
        }

        return AimDirection.Right;
    }

    /// <summary>
    /// Converts a linear value to decibels.
    /// </summary>
    /// <param name="linear">The linear value to convert.</param>
    /// <returns>The value in decibels.</returns>
    public static float LinearToDecibels(int linear)
    {
        float linearScaleRange = 20f;
        return Mathf.Log10((float)linear / linearScaleRange) * 20f;
    }

    /// <summary>
    /// Validates if the given string is empty.
    /// </summary>
    /// <param name="thisObject">The object being validated.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="stringToCheck">The string to check.</param>
    /// <returns>True if the string is empty, otherwise false.</returns>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (string.IsNullOrEmpty(stringToCheck))
        {
            Debug.Log($"{fieldName} is empty and must contain a value in object {thisObject.name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Validates if the given object is null.
    /// </summary>
    /// <param name="thisObject">The object being validated.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="objectToCheck">The object to check.</param>
    /// <returns>True if the object is null, otherwise false.</returns>
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, UnityEngine.Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log($"{fieldName} is null and must contain a value in object {thisObject.name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Validates if the given enumerable object contains null values or is empty.
    /// </summary>
    /// <param name="thisObject">The object being validated.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="enumerableObjectToCheck">The enumerable object to check.</param>
    /// <returns>True if the enumerable object contains null values or is empty, otherwise false.</returns>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        if (enumerableObjectToCheck == null)
        {
            Debug.Log($"{fieldName} is null in object {thisObject.name}");
            return true;
        }

        bool hasNullValues = false;
        int count = 0;

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log($"{fieldName} has null values in object {thisObject.name}");
                hasNullValues = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log($"{fieldName} has no values in object {thisObject.name}");
            return true;
        }

        return hasNullValues;
    }

    /// <summary>
    /// Validates if the given integer value is positive.
    /// </summary>
    /// <param name="thisObject">The object being validated.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="valueToCheck">The value to check.</param>
    /// <param name="isZeroAllowed">Indicates if zero is allowed.</param>
    /// <returns>True if the value is not positive, otherwise false.</returns>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, int valueToCheck, bool isZeroAllowed)
    {
        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log($"{fieldName} must contain a positive value or zero in object {thisObject.name}");
                return true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log($"{fieldName} must contain a positive value in object {thisObject.name}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Validates if the given float value is positive.
    /// </summary>
    /// <param name="thisObject">The object being validated.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="valueToCheck">The value to check.</param>
    /// <param name="isZeroAllowed">Indicates if zero is allowed.</param>
    /// <returns>True if the value is not positive, otherwise false.</returns>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log($"{fieldName} must contain a positive value or zero in object {thisObject.name}");
                return true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log($"{fieldName} must contain a positive value in object {thisObject.name}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Validates if the given range of float values is positive and the minimum value is less than or equal to the maximum value.
    /// </summary>
    /// <param name="thisObject">The object being validated.</param>
    /// <param name="fieldNameMinimum">The name of the minimum value field being validated.</param>
    /// <param name="valueToCheckMinimum">The minimum value to check.</param>
    /// <param name="fieldNameMaximum">The name of the maximum value field being validated.</param>
    /// <param name="valueToCheckMaximum">The maximum value to check.</param>
    /// <param name="isZeroAllowed">Indicates if zero is allowed.</param>
    /// <returns>True if the range is not valid, otherwise false.</returns>
    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, float valueToCheckMinimum,
        string fieldNameMaximum, float valueToCheckMaximum, bool isZeroAllowed)
    {
        bool error = false;

        if (valueToCheckMinimum > valueToCheckMaximum)
        {
            Debug.Log($"{fieldNameMinimum} must be less than or equal to {fieldNameMaximum} in object {thisObject.name}");
            error = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMinimum, valueToCheckMinimum, isZeroAllowed)) error = true;
        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMaximum, isZeroAllowed)) error = true;

        return error;
    }

    /// <summary>
    /// Gets the spawn position nearest to the player.
    /// </summary>
    /// <param name="playerPosition">The position of the player.</param>
    /// <returns>The nearest spawn position to the player.</returns>
    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Grid grid = currentRoom.instantiatedRoom.grid;

        Vector3 nearestSpawnPosition = new Vector3(float.MaxValue, float.MaxValue, 0f);

        foreach (Vector2Int spawnPositionGrid in currentRoom.spawnPositionArray)
        {
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);

            if (Vector3.Distance(spawnPositionWorld, playerPosition) < Vector3.Distance(nearestSpawnPosition, playerPosition))
            {
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;
    }

}
