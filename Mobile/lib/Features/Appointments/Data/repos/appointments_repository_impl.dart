import 'package:medlink/Features/Appointments/Data/datasources/appointments_remote_data_source.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';
import 'package:medlink/Features/Appointments/Domain/Repos/appointments_repository.dart';

class AppointmentsRepositoryImpl implements AppointmentsRepository {
  final AppointmentsRemoteDataSource remoteDataSource;

  AppointmentsRepositoryImpl({required this.remoteDataSource});

  @override
  Future<List<Appointment>> getMyAppointments() =>
      remoteDataSource.getMyAppointments();

  @override
  Future<void> cancel(String appointmentId) =>
      remoteDataSource.cancel(appointmentId);

  @override
  Future<void> updateNotes(String appointmentId, String notes) =>
      remoteDataSource.updateNotes(appointmentId, notes);
}
