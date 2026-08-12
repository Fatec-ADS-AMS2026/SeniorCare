// Espelha OperationalState (backend, Objects/Enums/OperationalState.cs) —
// serializado como integer (sem JsonStringEnumConverter global na API), mesma
// enumeração 0-3 pinada no backend.
enum OperationalState {
  AVAILABLE = 0,
  MAINTENANCE = 1,
  UNAVAILABLE = 2,
  DISABLED = 3,
}

export default OperationalState;
