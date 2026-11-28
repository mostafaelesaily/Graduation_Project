import 'dart:convert';

import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Appointments/Domain/Repos/appointments_repository.dart';
import 'package:medlink/Features/Booking/Presentation/Cubit/booking_state.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';

class BookingCubit extends Cubit<BookingState> {
  final ProviderRepository providerRepository;
  final AppointmentsRepository appointmentsRepository;
  final String providerId;

  BookingCubit({
    required this.providerRepository,
    required this.appointmentsRepository,
    required this.providerId,
  }) : super(BookingState(selectedDay: _today()));

  /// The API has no list of working hours for patients, so every half hour
  /// between these two is asked about.
  static const _firstHour = 8;
  static const _lastHour = 22;

  int _latestLoad = 0;

  static DateTime _today() {
    final now = DateTime.now();

    return DateTime(now.year, now.month, now.day);
  }

  Future<void> selectDay(DateTime day) async {
    emit(state.copyWith(selectedDay: day, clearSlot: true));
    await loadSlots();
  }

  void selectSlot(DateTime slot) {
    emit(state.copyWith(selectedSlot: slot));
  }

  Future<void> loadSlots() async {
    final loadId = ++_latestLoad;
    final day = state.selectedDay;

    emit(state.copyWith(slotsStatus: SlotsStatus.loading, slots: const []));

    try {
      final times = [
        for (var hour = _firstHour; hour < _lastHour; hour++)
          for (final minute in [0, 30])
            DateTime(day.year, day.month, day.day, hour, minute),
      ];

      final slots = await Future.wait(
        times.map((time) async {
          final isAvailable =
              time.isAfter(DateTime.now()) &&
              await providerRepository.isSlotAvailable(providerId, time);

          return TimeSlot(time: time, isAvailable: isAvailable);
        }),
      );
      if (isClosed || loadId != _latestLoad) return;

      // Drops the closed hours before the first and after the last free time.
      final first = slots.indexWhere((slot) => slot.isAvailable);
      final last = slots.lastIndexWhere((slot) => slot.isAvailable);

      emit(
        state.copyWith(
          slotsStatus: SlotsStatus.success,
          slots: first == -1 ? const [] : slots.sublist(first, last + 1),
        ),
      );
    } catch (_) {
      if (isClosed || loadId != _latestLoad) return;

      emit(state.copyWith(slotsStatus: SlotsStatus.failure));
    }
  }

  /// Notes are saved in a second request, because booking does not take them.
  Future<void> confirm(String notes) async {
    final slot = state.selectedSlot;
    if (slot == null || state.status == BookingStatus.loading) return;

    emit(state.copyWith(status: BookingStatus.loading));

    final String bookingId;

    try {
      bookingId = await providerRepository.book(providerId, slot);
    } catch (e) {
      if (isClosed) return;

      emit(
        state.copyWith(
          status: BookingStatus.failure,
          message: _getErrorMessage(e),
          clearSlot: true,
        ),
      );
      // The time may have just been taken by someone else.
      await loadSlots();
      return;
    }

    String? warning;

    if (notes.isNotEmpty) {
      try {
        await appointmentsRepository.updateNotes(bookingId, notes);
      } catch (_) {
        warning = 'Booked, but the notes were not saved.';
      }
    }

    if (isClosed) return;

    emit(state.copyWith(status: BookingStatus.success, message: warning));
  }

  String _getErrorMessage(Object error) {
    final message = error.toString();

    try {
      final data = jsonDecode(message.substring(message.indexOf('{')));

      return data['Message'].toString();
    } catch (_) {
      return 'Booking failed. Log in with a patient account and retry.';
    }
  }
}
