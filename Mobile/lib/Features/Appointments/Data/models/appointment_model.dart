import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';

/// One item of the data list returned by GET /api/Booking/my.
class AppointmentModel extends Appointment {
  const AppointmentModel({
    required super.id,
    required super.providerId,
    required super.date,
    required super.status,
    super.notes,
  });

  factory AppointmentModel.fromJson(Map<String, dynamic> json) {
    return AppointmentModel(
      id: json['id'],
      providerId: json['providerProfileId'],
      // Sent without a time zone: it is the same clock time the patient picked.
      date: DateTime.parse(json['bookingDate']),
      status: AppointmentStatus.values[json['status']],
      notes: json['notes'],
    );
  }
}
