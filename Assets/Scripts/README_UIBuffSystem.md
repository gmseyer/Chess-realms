# UI Buff System Documentation

## Overview
The UI Buff System provides visual indicators for status effects on chess pieces. When a piece has certain status effects (like Invulnerable, Ethereal, Bounty, etc.), corresponding UI icons will appear above the piece to make the status visible to players.

## Files Created

### 1. UIBuffManager.cs
- **Purpose**: Manages UI buff icons for individual chess pieces
- **Features**:
  - Automatically shows/hides buff icons based on status effects
  - Supports multiple buff icons simultaneously
  - Configurable icon positioning and scaling
  - Easy to extend for new status types

### 2. UIBuffTester.cs
- **Purpose**: Test script for debugging and testing the UI buff system
- **Features**:
  - Keyboard shortcuts for testing different buffs
  - Visual feedback in the game view
  - Easy way to add/remove buffs for testing

## Setup Instructions

### Step 1: Configure UIBuffManager
1. The UIBuffManager is automatically added to all chess pieces via the Chessman class
2. In the Unity Inspector, you'll need to assign the `invulnerableIconPrefab` to each piece's UIBuffManager component
3. Adjust the `iconOffset` and `iconScale` values as needed for proper positioning

### Step 2: Create Buff Icon Prefabs
1. Create UI prefabs for each status effect you want to display:
   - `InvulnerableIconPrefab` - for invulnerable status
   - `EtherealIconPrefab` - for ethereal status (when implemented)
   - `BountyIconPrefab` - for bounty status (when implemented)
2. These should be simple UI elements (Image components) with appropriate sprites

### Step 3: Test the System
1. Add the `UIBuffTester` script to any GameObject in your scene
2. Play the scene
3. Click on a chess piece to select it
4. Use the keyboard shortcuts to test different buffs:
   - `I` - Test Invulnerable buff
   - `E` - Test Ethereal buff
   - `B` - Test Bounty buff
   - `C` - Clear all buffs
   - `T` - Test all buffs at once

## How It Works

### Automatic Integration
The system is automatically integrated with the existing status system:
1. When `Chessman.UpdateVisualStatus()` is called, it also calls `UIBuffManager.UpdateBuffIcons()`
2. The UIBuffManager checks all active status effects
3. Icons are shown/hidden based on the current status

### Status Types Supported
Currently supported status types for UI icons:
- `StatusType.Invulnerable` - Shows invulnerable icon
- `StatusType.Ethereal` - Shows ethereal icon (when prefab is assigned)
- `StatusType.Bounty` - Shows bounty icon (when prefab is assigned)

### Adding New Status Types
To add support for new status types:

1. **Add the status type to UIBuffManager.cs**:
```csharp
// In GetIconPrefab method, add:
case StatusType.YourNewStatus:
    return yourNewStatusIconPrefab;
```

2. **Add the prefab field**:
```csharp
[Header("UI Buff Prefabs")]
public GameObject invulnerableIconPrefab;
public GameObject yourNewStatusIconPrefab; // Add this
```

3. **Update the UpdateBuffIcons method**:
```csharp
// Add check for your new status
bool hasYourNewStatus = statusManager.HasStatus(StatusType.YourNewStatus, game.turns);
UpdateBuffIcon(StatusType.YourNewStatus, hasYourNewStatus);
```

## Configuration Options

### Icon Positioning
- `iconOffset`: Vector3 offset from the piece's center position
- `iconScale`: Scale multiplier for all buff icons
- Default offset: `(0.3f, 0.3f, -2f)` - slightly above and to the right of the piece

### Icon Management
- Icons are automatically positioned relative to the piece
- Multiple icons can be active simultaneously
- Icons are destroyed when status effects expire
- Icons are cleared when the piece is destroyed

## Troubleshooting

### Icons Not Appearing
1. Check that the prefab is assigned in the UIBuffManager component
2. Verify that the piece has the status effect active
3. Check the Console for debug messages from UIBuffManager
4. Ensure the icon prefab has a visible sprite/image component

### Icons in Wrong Position
1. Adjust the `iconOffset` value in the UIBuffManager
2. Check that the icon prefab's pivot point is set correctly
3. Consider the piece's scale when setting offset values

### Performance Issues
1. Icons are only created/destroyed when status changes
2. The system uses object pooling concepts (reuse existing icons)
3. If you have many pieces with many buffs, consider limiting the number of simultaneous icons

## Future Enhancements

### Planned Features
- Support for more status types
- Animated buff icons
- Stack count display for multiple instances of the same status
- Different icon positions for different status types
- Sound effects when buffs are applied/removed

### Customization Options
- Per-status icon positioning
- Status-specific icon scaling
- Color coding for different status types
- Fade in/out animations for status changes

## Integration with Existing Systems

The UI Buff System integrates seamlessly with:
- **StatusManager**: Uses existing status tracking
- **Chessman**: Automatically updates when visual status changes
- **Game**: Respects turn-based status expiration
- **UIManager**: Works alongside existing UI systems

No changes to existing game logic are required - the system is purely additive and enhances the visual feedback for players.
