import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_state.dart';

class AppointmentsCubit extends Cubit<AppointmentsState> {
  AppointmentsCubit() : super(const AppointmentsState());

  void changeFilter(int chosenFilter) {
    emit(state.copyWith(chosenFilter: chosenFilter));
  }
}
