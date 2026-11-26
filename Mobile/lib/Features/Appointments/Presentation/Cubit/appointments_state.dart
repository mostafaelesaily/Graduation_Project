class AppointmentsState {
  final int chosenFilter;

  const AppointmentsState({this.chosenFilter = 0});

  AppointmentsState copyWith({chosenFilter}) {
    return AppointmentsState(chosenFilter: chosenFilter ?? this.chosenFilter);
  }
}
