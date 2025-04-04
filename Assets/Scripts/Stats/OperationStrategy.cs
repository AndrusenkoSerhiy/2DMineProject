namespace Stats {
  public interface IOperationStrategy {
    public float Calculate(float value);
  }

  public class AddOperation : IOperationStrategy {
    private readonly float value;

    public AddOperation(float value) {
      this.value = value;
    }

    public float Calculate(float value) => value + this.value;
    public new string ToString() => $"Add_{value}";
  }

  public class MultiplyOperation : IOperationStrategy {
    private readonly float value;

    public MultiplyOperation(float value) {
      this.value = value;
    }

    public float Calculate(float value) => value * this.value;
    public new string ToString() => $"Multiply_{value}";
  }
}