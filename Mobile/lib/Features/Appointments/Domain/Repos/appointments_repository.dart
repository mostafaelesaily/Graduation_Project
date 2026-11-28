import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';

abstract class AppointmentsRepository {
  Future<List<Appointment>> getMyAppointments();

  Future<void> cancel(String appointmentId);

  Future<void> updateNotes(String appointmentId, String notes);
}
