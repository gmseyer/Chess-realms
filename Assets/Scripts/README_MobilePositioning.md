# Mobile Positioning Fix Setup Guide

## Problem
Your Unity game works correctly in the editor at 1440x3040 resolution, but when built as an APK and run on a Samsung Galaxy S10 with the same resolution, the game appears pushed down by about 100px.

## Solution
I've created a dynamic positioning system that automatically adjusts for different screen configurations and Android device characteristics.

## Files Created/Modified

### 1. ScreenManager.cs (NEW)
- Handles dynamic positioning calculations
- Accounts for screen aspect ratio differences
- Compensates for Android status bar and navigation bar
- Provides fallback to original hardcoded values if needed

### 2. Chessman.cs (MODIFIED)
- Updated `SetCoords()` method to use dynamic positioning
- Updated `MovePlateSpawn()` and `MovePlateAttackSpawn()` methods
- Maintains backward compatibility with fallback values

### 3. PositioningAdjuster.cs (NEW)
- Provides real-time positioning adjustment during testing
- Includes keyboard shortcuts for quick adjustments
- Can be attached to UI buttons for runtime adjustment

## Setup Instructions

### Step 1: Add ScreenManager to Scene
1. Create an empty GameObject in your scene
2. Name it "ScreenManager"
3. Attach the `ScreenManager.cs` script to it
4. The script will automatically detect screen dimensions and calculate adjustments

### Step 2: Test in Unity Editor
1. Set your Game view to 1440x3040 resolution
2. Play the scene
3. Check the Console for ScreenManager debug logs
4. Verify that pieces are positioned correctly

### Step 3: Test on Android Device
1. Build and install the APK on your Samsung Galaxy S10
2. Check the Console logs (if accessible) for screen dimension information
3. If positioning is still off, use the PositioningAdjuster

### Step 4: Fine-tune Positioning (if needed)
1. Add the `PositioningAdjuster.cs` script to a GameObject in your scene
2. Create UI buttons and assign them to the script's button references
3. Use the buttons or keyboard shortcuts to adjust positioning:
   - Arrow Keys: Adjust position
   - R Key: Reset to default
4. The adjustments will be applied in real-time

## Keyboard Shortcuts (for testing)
- ↑ Arrow: Move up
- ↓ Arrow: Move down  
- ← Arrow: Move left
- → Arrow: Move right
- R: Reset to default

## How It Works

1. **Screen Detection**: ScreenManager detects the actual screen dimensions and aspect ratio
2. **Aspect Ratio Compensation**: Calculates differences from your target 1440x3040 ratio
3. **Android Safe Area**: Accounts for status bar and navigation bar on Android devices
4. **Dynamic Positioning**: All pieces and move plates use the calculated positions instead of hardcoded values
5. **Fallback System**: If ScreenManager isn't available, falls back to original hardcoded values

## Expected Results

- Game should appear correctly positioned on your Samsung Galaxy S10
- No more 100px downward shift
- Consistent positioning across different Android devices
- Maintains compatibility with Unity Editor

## Troubleshooting

If positioning is still incorrect:
1. Check Console logs for ScreenManager debug information
2. Use PositioningAdjuster to fine-tune in real-time
3. Adjust the `boardOffsetX` and `boardOffsetY` values in ScreenManager
4. Consider device-specific adjustments in the ScreenManager script

## Notes

- The system maintains backward compatibility
- Original hardcoded values are preserved as fallback
- ScreenManager is a singleton and persists across scene loads
- All positioning calculations are done in real-time
