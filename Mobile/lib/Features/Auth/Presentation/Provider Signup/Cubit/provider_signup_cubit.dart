import 'package:file_picker/file_picker.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';

import 'provider_signup_state.dart';

class ProviderSignupCubit extends Cubit<ProviderSignupState> {
  final AuthRepository authRepository;

  ProviderSignupCubit({required this.authRepository})
    : super(const ProviderSignupState());

  static const int maxFileSize = 10 * 1024 * 1024; // 10 MB

  static const List<String> serviceTypes = [
    'Doctor',
    'Clinic',
    'Laboratory',
    'Radiology Center',
  ];

  void selectServiceType(String? value) {
    emit(
      state.copyWith(
        selectedServiceType: value,
        clearServiceType: value == null,
        clearError: true,
        status: ProviderSignupStatus.initial,
      ),
    );
  }

  Future<void> pickLicense() async {
    if (state.isPickingFile || state.isSubmitting) return;

    emit(
      state.copyWith(
        status: ProviderSignupStatus.pickingFile,
        clearError: true,
      ),
    );

    try {
      final file = await FilePicker.pickFile(
        type: FileType.custom,
        allowedExtensions: ['pdf', 'jpg', 'jpeg', 'png'],
      );

      if (isClosed) return;

      // User cancelled file selection.
      if (file == null) {
        emit(state.copyWith(status: ProviderSignupStatus.initial));
        return;
      }

      final fileSize = await file.length();
      if (fileSize! > maxFileSize) {
        emit(
          state.copyWith(
            status: ProviderSignupStatus.failure,
            errorMessage: 'File size must not exceed 10 MB.',
          ),
        );
        return;
      }

      emit(
        state.copyWith(
          licenseFile: file,
          licenseSize: fileSize,
          status: ProviderSignupStatus.initial,
          clearError: true,
        ),
      );
    } on MissingPluginException {
      _emitFailure(
        'File picker is not registered. Please restart the application.',
      );
    } on PlatformException catch (e) {
      _emitFailure(e.message ?? 'Unable to open the file picker.');
    } on Exception catch (e) {
      _emitFailure(_getErrorMessage(e));
    }
  }

  Future<void> signup(RegisterRequestModel model, PlatformFile file) async {
    if (state.isSubmitting || state.isPickingFile) return;

    final license = state.licenseFile;

    if (state.selectedServiceType == null) {
      _emitFailure('Please select a service type.');
      return;
    }

    if (license == null) {
      _emitFailure('Please upload your medical license.');
      return;
    }

    emit(
      state.copyWith(status: ProviderSignupStatus.submitting, clearError: true),
    );

    try {
      await authRepository.providerRegister(model, file);

      if (isClosed) return;

      emit(
        state.copyWith(status: ProviderSignupStatus.success, clearError: true),
      );
    } on Exception catch (e) {
      if (isClosed) return;

      emit(
        state.copyWith(
          status: ProviderSignupStatus.failure,
          errorMessage: _getErrorMessage(e),
        ),
      );
    }
  }

  void _emitFailure(String message) {
    if (isClosed) return;

    emit(
      state.copyWith(
        status: ProviderSignupStatus.failure,
        errorMessage: message,
      ),
    );
  }

  String _getErrorMessage(Object error) {
    final message = error.toString();

    if (message.startsWith('Exception: ')) {
      return message.substring('Exception: '.length);
    }

    return 'Signup failed. Please try again.';
  }
}
