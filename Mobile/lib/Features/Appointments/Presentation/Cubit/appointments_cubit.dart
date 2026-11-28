import 'dart:convert';

import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';
import 'package:medlink/Features/Appointments/Domain/Repos/appointments_repository.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_state.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';

class AppointmentsCubit extends Cubit<AppointmentsState> {
  final AppointmentsRepository appointmentsRepository;
  final ProviderRepository providerRepository;

  AppointmentsCubit({
    required this.appointmentsRepository,
    required this.providerRepository,
  }) : super(const AppointmentsState());

  int _latestLoad = 0;

  void changeFilter(AppointmentFilter filter) {
    emit(state.copyWith(filter: filter));
  }

  Future<void> load() async {
    final loadId = ++_latestLoad;

    // Keep showing the old list while refreshing it.
    if (state.appointments.isEmpty) {
      emit(state.copyWith(status: AppointmentsStatus.loading));
    }

    try {
      final appointments = await appointmentsRepository.getMyAppointments();
      final providers = await _loadProviders();
      if (isClosed || loadId != _latestLoad) return;

      emit(
        state.copyWith(
          status: AppointmentsStatus.success,
          appointments: appointments.map((appointment) {
            final provider = providers[appointment.providerId];
            if (provider == null) return appointment;

            return appointment.withProvider(
              providerName: provider.serviceName,
              address: provider.address,
              phoneNumber: provider.phoneNumber,
            );
          }).toList(),
        ),
      );
    } catch (_) {
      if (isClosed || loadId != _latestLoad) return;

      emit(state.copyWith(status: AppointmentsStatus.failure));
    }
  }

  /// Returns null on success, otherwise the message to show.
  Future<String?> cancel(String appointmentId) =>
      _runAction(() => appointmentsRepository.cancel(appointmentId));

  /// Returns null on success, otherwise the message to show.
  Future<String?> updateNotes(String appointmentId, String notes) => _runAction(
    () => appointmentsRepository.updateNotes(appointmentId, notes),
  );

  Future<String?> _runAction(Future<void> Function() action) async {
    try {
      await action();
    } catch (e) {
      return _getErrorMessage(e);
    }

    if (!isClosed) await load();
    return null;
  }

  /// A failure here only hides the provider details, not the appointments.
  Future<Map<String, ProviderProfile>> _loadProviders() async {
    try {
      final providers = await providerRepository.getProviders(pageSize: 200);

      return {for (final provider in providers) provider.id: provider};
    } catch (_) {
      return const {};
    }
  }

  String _getErrorMessage(Object error) {
    final message = error.toString();

    try {
      final data = jsonDecode(message.substring(message.indexOf('{')));

      return data['Message'].toString();
    } catch (_) {
      return 'Something went wrong. Please try again.';
    }
  }
}
