import 'dart:convert';

import 'package:medlink/Core/Network/api_client.dart';
import 'package:medlink/Features/Appointments/Data/models/appointment_model.dart';

class AppointmentsRemoteDataSource {
  final ApiClient client;

  AppointmentsRemoteDataSource({required this.client});

  /// Reads every page, so the filters work on the full list.
  Future<List<AppointmentModel>> getMyAppointments() async {
    const path = '/api/Booking/my';
    final appointments = <AppointmentModel>[];
    var page = 1;

    while (true) {
      final response = await client.get(
        path,
        queryParameters: {'pageNum': '$page', 'pageSize': '50'},
      );

      if (response.statusCode != 200) {
        throw Exception('Loading appointments failed: ${response.body}');
      }

      final decoded = jsonDecode(response.body);
      final items = decoded['data'] as List;

      appointments.addAll(items.map((item) => AppointmentModel.fromJson(item)));

      if (items.isEmpty || appointments.length >= decoded['totalCount']) {
        return appointments;
      }

      page++;
    }
  }

  Future<void> cancel(String appointmentId) async {
    final response = await client.patch('/api/Booking/$appointmentId/cancel');

    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception('Cancelling failed: ${response.body}');
    }
  }

  Future<void> updateNotes(String appointmentId, String notes) async {
    final response = await client.put(
      '/api/Booking/$appointmentId/notes',
      body: {'notes': notes},
    );

    if (response.statusCode != 204 && response.statusCode != 200) {
      throw Exception('Saving notes failed: ${response.body}');
    }
  }
}
