import 'dart:convert';

import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/Cubit/provider_profile_state.dart';

class ProviderProfileCubit extends Cubit<ProviderProfileState> {
  final ProviderRepository providerRepository;

  ProviderProfileCubit({required this.providerRepository})
    : super(const ProviderProfileState());

  Future<void> book({
    required String providerId,
    required DateTime slot,
  }) async {
    if (state.status == BookingStatus.loading) return;

    emit(state.copyWith(status: BookingStatus.loading, clearError: true));

    try {
      final isAvailable = await providerRepository.isSlotAvailable(
        providerId,
        slot,
      );

      if (!isAvailable) {
        emit(
          state.copyWith(
            status: BookingStatus.failure,
            errorMessage: {'message': 'This time is not available.'},
          ),
        );
        return;
      }

      await providerRepository.book(providerId, slot);
      emit(state.copyWith(status: BookingStatus.success, clearError: true));
    } on Exception catch (e) {
      emit(
        state.copyWith(
          status: BookingStatus.failure,
          errorMessage: _getErrorMessage(e),
        ),
      );
    }
  }

  Map<String, dynamic> _getErrorMessage(Object error) {
    final message = error.toString();

    try {
      final data = jsonDecode(message.substring(message.indexOf('{')));

      return {'errorCode': data['StatusCode'], 'message': data['Message']};
    } catch (_) {
      return {
        'message': 'Booking failed. Log in with a patient account and retry.',
      };
    }
  }
}
