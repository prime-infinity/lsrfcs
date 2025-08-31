# Design Document

## Overview

This design enhances the existing LaserFocus WPF application UI to provide a modern, visually appealing, and user-friendly interface. The enhancement focuses on improving visual aesthetics, adding helpful explanations, implementing better layout organization, and introducing privacy features for blocked website management.

The design maintains the existing functional architecture while significantly improving the user experience through modern styling, better information hierarchy, and enhanced usability features.

## Architecture

### UI Architecture Pattern

- **Existing Pattern**: WPF with XAML and code-behind, data binding with ObservableCollections
- **Enhancement Approach**: Maintain existing architecture while adding:
  - Custom styles and themes using WPF ResourceDictionary
  - Enhanced data templates for better visual presentation
  - Additional UI state management for website visibility toggle
  - Improved layout containers and visual grouping

### Visual Design System

- **Color Scheme**: Modern blue/teal accent colors with neutral grays
- **Typography**: Consistent font hierarchy with clear size differentiation
- **Spacing**: Standardized margins and padding using 8px grid system
- **Visual Hierarchy**: Clear section separation with cards/panels and proper contrast

## Components and Interfaces

### 1. Enhanced Main Window Layout

**Current Structure**: Basic Grid with GroupBox containers
**Enhanced Structure**:

- Modern card-based layout with rounded corners and shadows
- Improved header with application branding and status indicators
- Better visual separation between functional areas
- Responsive layout that adapts to window resizing

### 2. Website Blocking Section Enhancement

**Visual Improvements**:

- Modern input controls with placeholder text and validation styling
- Enhanced blocked websites list with better typography and spacing
- Privacy toggle functionality for hiding/showing website addresses

**Privacy Feature Components**:

- `IsWebsiteListVisible` property (bool) for controlling visibility state
- Toggle button that switches between "Show Blocked Websites" and "Hide Blocked Websites"
- Masked display showing count and placeholder text when hidden
- Preserved functionality for add/remove operations regardless of visibility state

**Data Template Enhancement**:

```xml
<!-- Enhanced website list item with conditional visibility -->
<DataTemplate>
    <Border Style="{StaticResource WebsiteItemStyle}">
        <Grid>
            <TextBlock Text="{Binding}" Visibility="{Binding IsVisible}" />
            <TextBlock Text="••••••••••••••" Visibility="{Binding IsHidden}" />
            <Button Content="Remove" Style="{StaticResource RemoveButtonStyle}" />
        </Grid>
    </Border>
</DataTemplate>
```

### 3. Process Monitoring Section Enhancement

**Visual Improvements**:

- Enhanced DataGrid styling with alternating row colors
- Better status indicators with color-coded badges
- Improved column headers with clear descriptions
- Modern button styling for Start/Stop controls

**Status Indicators**:

- Green badge for "Allowed" processes
- Red badge for "Blocked" processes
- Consistent iconography and color coding

### 4. Enhanced Status and Information Display

**Information Panels**:

- Contextual help text explaining each section's purpose
- Tooltips providing additional guidance on controls
- Status bar with better formatting and color-coded messages
- Progress indicators for long-running operations

**Help Text Examples**:

- Website Blocking: "Block distracting websites system-wide across all browsers"
- Process Monitoring: "Automatically close non-essential applications while preserving development tools"

## Data Models

### Enhanced UI State Management

**New Properties for MainWindow**:

```csharp
// Website visibility toggle
private bool _isWebsiteListVisible = false;
public bool IsWebsiteListVisible
{
    get => _isWebsiteListVisible;
    set
    {
        _isWebsiteListVisible = value;
        OnPropertyChanged(nameof(IsWebsiteListVisible));
        OnPropertyChanged(nameof(WebsiteToggleButtonText));
        OnPropertyChanged(nameof(WebsiteListDisplayText));
    }
}

// Dynamic button text
public string WebsiteToggleButtonText =>
    IsWebsiteListVisible ? "Hide Blocked Websites" : "Show Blocked Websites";

// Display text for hidden state
public string WebsiteListDisplayText =>
    IsWebsiteListVisible ? "" : $"{BlockedWebsites.Count} websites blocked (hidden for privacy)";
```

**Configuration Persistence**:

- Save UI preferences (like website visibility state) to app-settings.json
- Restore user preferences on application startup

## Styling and Theme System

### Resource Dictionary Structure

```xml
<ResourceDictionary>
    <!-- Color Palette -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="#2196F3"/>
    <SolidColorBrush x:Key="AccentBrush" Color="#00BCD4"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="#FAFAFA"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="#212121"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="#757575"/>

    <!-- Card Style -->
    <Style x:Key="CardStyle" TargetType="Border">
        <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
        <Setter Property="CornerRadius" Value="8"/>
        <Setter Property="Padding" Value="20"/>
        <Setter Property="Margin" Value="10"/>
        <Setter Property="Effect">
            <Setter.Value>
                <DropShadowEffect BlurRadius="10" ShadowDepth="2" Opacity="0.1"/>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- Button Styles -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Template">
            <!-- Custom template with hover effects -->
        </Setter>
    </Style>
</ResourceDictionary>
```

### Typography System

- **Header**: 24px, SemiBold, Primary color
- **Section Title**: 16px, SemiBold, Primary text color
- **Body Text**: 14px, Regular, Primary text color
- **Caption**: 12px, Regular, Secondary text color
- **Button Text**: 14px, SemiBold

## Error Handling

### Enhanced User Feedback

- **Visual Error States**: Red border and background tint for invalid inputs
- **Success Indicators**: Green checkmarks and positive feedback messages
- **Loading States**: Progress indicators for operations that take time
- **Contextual Help**: Tooltips and help text to prevent errors

### Privilege Status Display

- **Admin Mode Indicator**: Green badge showing "Full Access" when running as administrator
- **Limited Mode Warning**: Orange warning indicator when running with limited privileges
- **Feature Availability**: Visual indicators showing which features are available based on privileges

## Testing Strategy

### Visual Testing

- **Cross-Resolution Testing**: Verify layout works on different screen sizes and DPI settings
- **Theme Consistency**: Ensure all UI elements follow the design system
- **Accessibility Testing**: Verify color contrast ratios meet WCAG guidelines
- **Animation Performance**: Test smooth transitions and hover effects

### Functional Testing

- **Website Visibility Toggle**: Test show/hide functionality preserves list operations
- **State Persistence**: Verify UI preferences are saved and restored correctly
- **Responsive Layout**: Test window resizing and minimum size constraints
- **Keyboard Navigation**: Ensure all controls are accessible via keyboard

### Integration Testing

- **Data Binding**: Verify all ObservableCollection updates reflect in UI
- **Configuration Sync**: Test that UI state changes are properly persisted
- **Error State Handling**: Verify error states display correctly and recover gracefully

## Implementation Phases

### Phase 1: Core Styling and Layout

- Implement resource dictionary with color scheme and base styles
- Update main window layout with card-based design
- Add enhanced typography and spacing

### Phase 2: Website Privacy Feature

- Implement website visibility toggle functionality
- Add masked display for hidden websites
- Integrate toggle state persistence

### Phase 3: Enhanced Information and Help

- Add contextual help text and tooltips
- Implement improved status indicators
- Add privilege status display

### Phase 4: Polish and Refinement

- Add hover effects and micro-animations
- Implement loading states and progress indicators
- Final accessibility and usability improvements

## Accessibility Considerations

- **Color Contrast**: All text meets WCAG AA standards (4.5:1 ratio minimum)
- **Keyboard Navigation**: Full keyboard accessibility for all interactive elements
- **Screen Reader Support**: Proper ARIA labels and semantic markup
- **Focus Indicators**: Clear visual focus indicators for keyboard navigation
- **Text Scaling**: UI remains functional at 150% text scaling
