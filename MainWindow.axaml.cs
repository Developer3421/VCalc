using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace VCalc;

public partial class MainWindow : Window
{
    private double _currentValue = 0;
    private string _currentOperator = "";
    private bool _isNewEntry = true;
    private string _expression = "";

    // UI Elements
    private TextBlock _resultDisplay = null!;
    private TextBlock _expressionDisplay = null!;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get display elements
        _resultDisplay = this.FindControl<TextBlock>("ResultDisplay")!;
        _expressionDisplay = this.FindControl<TextBlock>("ExpressionDisplay")!;

        // Window control buttons
        var newWindowButton = this.FindControl<Button>("NewWindowButton");
        var minimizeButton = this.FindControl<Button>("MinimizeButton");
        var closeButton = this.FindControl<Button>("CloseButton");
        var titleBar = this.FindControl<Grid>("TitleBar");

        if (newWindowButton != null) newWindowButton.Click += OnNewWindowClick;
        if (minimizeButton != null) minimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        if (closeButton != null) closeButton.Click += (_, _) => Close();
        if (titleBar != null) titleBar.PointerPressed += (_, e) => { if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e); };

        // Number buttons
        SetupNumberButton("Button0", "0");
        SetupNumberButton("Button1", "1");
        SetupNumberButton("Button2", "2");
        SetupNumberButton("Button3", "3");
        SetupNumberButton("Button4", "4");
        SetupNumberButton("Button5", "5");
        SetupNumberButton("Button6", "6");
        SetupNumberButton("Button7", "7");
        SetupNumberButton("Button8", "8");
        SetupNumberButton("Button9", "9");

        // Decimal button
        var decimalButton = this.FindControl<Button>("ButtonDecimal");
        if (decimalButton != null) decimalButton.Click += OnDecimalClick;

        // Operator buttons
        SetupOperatorButton("ButtonAdd", "+");
        SetupOperatorButton("ButtonSubtract", "−");
        SetupOperatorButton("ButtonMultiply", "×");
        SetupOperatorButton("ButtonDivide", "÷");
        SetupOperatorButton("ButtonPower", "^");

        // Function buttons
        var equalsButton = this.FindControl<Button>("ButtonEquals");
        if (equalsButton != null) equalsButton.Click += OnEqualsClick;

        var clearButton = this.FindControl<Button>("ButtonC");
        if (clearButton != null) clearButton.Click += OnClearClick;

        var clearEntryButton = this.FindControl<Button>("ButtonCE");
        if (clearEntryButton != null) clearEntryButton.Click += OnClearEntryClick;

        var backspaceButton = this.FindControl<Button>("ButtonBackspace");
        if (backspaceButton != null) backspaceButton.Click += OnBackspaceClick;

        var negateButton = this.FindControl<Button>("ButtonNegate");
        if (negateButton != null) negateButton.Click += OnNegateClick;

        // Scientific functions
        var sinButton = this.FindControl<Button>("ButtonSin");
        if (sinButton != null) sinButton.Click += OnSinClick;

        var cosButton = this.FindControl<Button>("ButtonCos");
        if (cosButton != null) cosButton.Click += OnCosClick;

        var tanButton = this.FindControl<Button>("ButtonTan");
        if (tanButton != null) tanButton.Click += OnTanClick;

        var logButton = this.FindControl<Button>("ButtonLog");
        if (logButton != null) logButton.Click += OnLogClick;

        var lnButton = this.FindControl<Button>("ButtonLn");
        if (lnButton != null) lnButton.Click += OnLnClick;

        var piButton = this.FindControl<Button>("ButtonPi");
        if (piButton != null) piButton.Click += OnPiClick;

        var eButton = this.FindControl<Button>("ButtonE");
        if (eButton != null) eButton.Click += OnEClick;

        // Keyboard support
        KeyDown += OnKeyDown;
    }

    private void OnNewWindowClick(object? sender, RoutedEventArgs e)
    {
        var newWindow = new MainWindow();
        newWindow.Show();
    }

    private void SetupNumberButton(string name, string digit)
    {
        var button = this.FindControl<Button>(name);
        if (button != null)
        {
            button.Click += (_, _) => AppendDigit(digit);
        }
    }

    private void SetupOperatorButton(string name, string op)
    {
        var button = this.FindControl<Button>(name);
        if (button != null)
        {
            button.Click += (_, _) => SetOperator(op);
        }
    }

    private void AppendDigit(string digit)
    {
        if (_isNewEntry)
        {
            _resultDisplay.Text = digit;
            _isNewEntry = false;
        }
        else
        {
            if (_resultDisplay.Text == "0" && digit != "0")
                _resultDisplay.Text = digit;
            else if (_resultDisplay.Text != "0")
                _resultDisplay.Text += digit;
        }
    }

    private void OnDecimalClick(object? sender, RoutedEventArgs e)
    {
        if (_isNewEntry)
        {
            _resultDisplay.Text = "0,";
            _isNewEntry = false;
        }
        else if (!_resultDisplay.Text!.Contains(","))
        {
            _resultDisplay.Text += ",";
        }
    }

    private void SetOperator(string op)
    {
        if (!_isNewEntry && !string.IsNullOrEmpty(_currentOperator))
        {
            Calculate();
        }
        
        _currentValue = ParseDisplay();
        _currentOperator = op;
        _expression = FormatNumber(_currentValue) + " " + op;
        _expressionDisplay.Text = _expression;
        _isNewEntry = true;
    }

    private void OnEqualsClick(object? sender, RoutedEventArgs e)
    {
        Calculate();
        _expression = "";
        _expressionDisplay.Text = "";
        _currentOperator = "";
        _isNewEntry = true;
    }

    private void Calculate()
    {
        if (string.IsNullOrEmpty(_currentOperator)) return;

        double secondValue = ParseDisplay();
        double result = 0;

        switch (_currentOperator)
        {
            case "+":
                result = _currentValue + secondValue;
                break;
            case "−":
                result = _currentValue - secondValue;
                break;
            case "×":
                result = _currentValue * secondValue;
                break;
            case "÷":
                if (secondValue == 0)
                {
                    _resultDisplay.Text = "😵";
                    _isNewEntry = true;
                    return;
                }
                result = _currentValue / secondValue;
                break;
            case "^":
                result = Math.Pow(_currentValue, secondValue);
                break;
        }

        _currentValue = result;
        _resultDisplay.Text = FormatNumber(result);
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        _currentValue = 0;
        _currentOperator = "";
        _expression = "";
        _isNewEntry = true;
        _resultDisplay.Text = "0";
        _expressionDisplay.Text = "";
    }

    private void OnClearEntryClick(object? sender, RoutedEventArgs e)
    {
        _resultDisplay.Text = "0";
        _isNewEntry = true;
    }

    private void OnBackspaceClick(object? sender, RoutedEventArgs e)
    {
        if (_isNewEntry) return;
        
        var text = _resultDisplay.Text;
        if (text != null && text.Length > 1)
        {
            _resultDisplay.Text = text.Substring(0, text.Length - 1);
        }
        else
        {
            _resultDisplay.Text = "0";
            _isNewEntry = true;
        }
    }

    private void OnNegateClick(object? sender, RoutedEventArgs e)
    {
        var value = ParseDisplay();
        value = -value;
        _resultDisplay.Text = FormatNumber(value);
    }

    // Scientific functions
    private void OnSinClick(object? sender, RoutedEventArgs e)
    {
        var value = ParseDisplay();
        // Convert degrees to radians
        var radians = value * Math.PI / 180;
        _resultDisplay.Text = FormatNumber(Math.Sin(radians));
        _isNewEntry = true;
    }

    private void OnCosClick(object? sender, RoutedEventArgs e)
    {
        var value = ParseDisplay();
        var radians = value * Math.PI / 180;
        _resultDisplay.Text = FormatNumber(Math.Cos(radians));
        _isNewEntry = true;
    }

    private void OnTanClick(object? sender, RoutedEventArgs e)
    {
        var value = ParseDisplay();
        var radians = value * Math.PI / 180;
        // Check for undefined values (90, 270, etc.)
        if (Math.Abs(Math.Cos(radians)) < 1e-10)
        {
            _resultDisplay.Text = "😵";
            _isNewEntry = true;
            return;
        }
        _resultDisplay.Text = FormatNumber(Math.Tan(radians));
        _isNewEntry = true;
    }

    private void OnLogClick(object? sender, RoutedEventArgs e)
    {
        var value = ParseDisplay();
        if (value <= 0)
        {
            _resultDisplay.Text = "😵";
            _isNewEntry = true;
            return;
        }
        _resultDisplay.Text = FormatNumber(Math.Log10(value));
        _isNewEntry = true;
    }

    private void OnLnClick(object? sender, RoutedEventArgs e)
    {
        var value = ParseDisplay();
        if (value <= 0)
        {
            _resultDisplay.Text = "😵";
            _isNewEntry = true;
            return;
        }
        _resultDisplay.Text = FormatNumber(Math.Log(value));
        _isNewEntry = true;
    }

    private void OnPiClick(object? sender, RoutedEventArgs e)
    {
        _resultDisplay.Text = FormatNumber(Math.PI);
        _isNewEntry = true;
    }

    private void OnEClick(object? sender, RoutedEventArgs e)
    {
        _resultDisplay.Text = FormatNumber(Math.E);
        _isNewEntry = true;
    }

    private double ParseDisplay()
    {
        var text = _resultDisplay.Text?.Replace(",", ".") ?? "0";
        if (text == "😵") return 0;
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }

    private string FormatNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return "😵";
        
        // Format with comma as decimal separator
        var result = value.ToString("G15", CultureInfo.InvariantCulture).Replace(".", ",");
        return result;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D0:
            case Key.NumPad0:
                AppendDigit("0");
                break;
            case Key.D1:
            case Key.NumPad1:
                AppendDigit("1");
                break;
            case Key.D2:
            case Key.NumPad2:
                AppendDigit("2");
                break;
            case Key.D3:
            case Key.NumPad3:
                AppendDigit("3");
                break;
            case Key.D4:
            case Key.NumPad4:
                AppendDigit("4");
                break;
            case Key.D5:
            case Key.NumPad5:
                AppendDigit("5");
                break;
            case Key.D6:
            case Key.NumPad6:
                AppendDigit("6");
                break;
            case Key.D7:
            case Key.NumPad7:
                AppendDigit("7");
                break;
            case Key.D8:
            case Key.NumPad8:
                AppendDigit("8");
                break;
            case Key.D9:
            case Key.NumPad9:
                AppendDigit("9");
                break;
            case Key.Add:
            case Key.OemPlus:
                SetOperator("+");
                break;
            case Key.Subtract:
            case Key.OemMinus:
                SetOperator("−");
                break;
            case Key.Multiply:
                SetOperator("×");
                break;
            case Key.Divide:
            case Key.OemQuestion:
                SetOperator("÷");
                break;
            case Key.Enter:
                OnEqualsClick(null, null!);
                break;
            case Key.Back:
                OnBackspaceClick(null, null!);
                break;
            case Key.Escape:
                OnClearClick(null, null!);
                break;
            case Key.Delete:
                OnClearEntryClick(null, null!);
                break;
            case Key.Decimal:
            case Key.OemComma:
            case Key.OemPeriod:
                OnDecimalClick(null, null!);
                break;
        }
    }
}