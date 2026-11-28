import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';

enum AppointmentsStatus { loading, success, failure }

class AppointmentsState {
  final AppointmentsStatus status;
  final List<Appointment> appointments;
  final AppointmentFilter filter;

  const AppointmentsState({
    this.status = AppointmentsStatus.loading,
    this.appointments = const [],
    this.filter = AppointmentFilter.upcoming,
  });

  /// Upcoming appointments are ordered nearest first, the others newest first.
  List<Appointment> byFilter(AppointmentFilter filter) {
    final list = appointments.where((a) => a.filter == filter).toList();

    list.sort(
      (a, b) => filter == AppointmentFilter.upcoming
          ? a.date.compareTo(b.date)
          : b.date.compareTo(a.date),
    );

    return list;
  }

  AppointmentsState copyWith({
    AppointmentsStatus? status,
    List<Appointment>? appointments,
    AppointmentFilter? filter,
  }) {
    return AppointmentsState(
      status: status ?? this.status,
      appointments: appointments ?? this.appointments,
      filter: filter ?? this.filter,
    );
  }
}
