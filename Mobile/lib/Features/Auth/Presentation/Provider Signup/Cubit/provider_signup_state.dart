import 'package:file_picker/file_picker.dart';

enum ProviderSignupStatus { initial, pickingFile, submitting, success, failure }

class ProviderSignupState {
  final String? selectedServiceType;
  final PlatformFile? licenseFile;
  final int? licenseSize;
  final ProviderSignupStatus status;
  final String? errorMessage;

  const ProviderSignupState({
    this.selectedServiceType,
    this.licenseFile,
    this.licenseSize,
    this.status = ProviderSignupStatus.initial,
    this.errorMessage,
  });

  bool get isSubmitting => status == ProviderSignupStatus.submitting;

  bool get isPickingFile => status == ProviderSignupStatus.pickingFile;

  bool get hasLicense => licenseFile != null;

  ProviderSignupState copyWith({
    String? selectedServiceType,
    PlatformFile? licenseFile,
    int? licenseSize,
    ProviderSignupStatus? status,
    String? errorMessage,
    bool clearServiceType = false,
    bool clearLicense = false,
    bool clearError = false,
  }) {
    return ProviderSignupState(
      selectedServiceType: clearServiceType
          ? null
          : selectedServiceType ?? this.selectedServiceType,

      licenseFile: clearLicense ? null : licenseFile ?? this.licenseFile,

      licenseSize: clearLicense ? null : licenseSize ?? this.licenseSize,

      status: status ?? this.status,

      errorMessage: clearError ? null : errorMessage ?? this.errorMessage,
    );
  }
}
