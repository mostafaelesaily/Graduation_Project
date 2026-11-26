import 'dart:io';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart';

import 'package:medlink/Core/Shared/input_field.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';

import 'package:medlink/Features/Auth/Presentation/Provider Signup/Cubit/provider_signup_cubit.dart';
import 'package:medlink/Features/Auth/Presentation/Provider Signup/Cubit/provider_signup_state.dart';
import 'package:medlink/Features/Auth/Widgets/verification_scaffold.dart';

class ProviderSignup extends StatefulWidget {
  ProviderSignup({super.key, required this.model});

  RegisterRequestModel model;

  @override
  State<ProviderSignup> createState() => _ProviderSignupState();
}

class _ProviderSignupState extends State<ProviderSignup> {
  final _formKey = GlobalKey<FormState>();

  final _serviceNameController = TextEditingController();
  final _locationController = TextEditingController();
  final _phoneController = TextEditingController();
  final _postalController = TextEditingController();
  final _taxController = TextEditingController();

  ProviderSignupCubit get _cubit => context.read<ProviderSignupCubit>();

  @override
  void dispose() {
    _serviceNameController.dispose();
    _locationController.dispose();
    _phoneController.dispose();
    _postalController.dispose();
    _taxController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    FocusScope.of(context).unfocus();

    // if (!_formKey.currentState!.validate()) return;

    // RegisterRequestModel model = RegisterRequestModel(
    //   name: widget.model!.name,
    //   email: widget.model!.email,
    //   phone: widget.model!.phone,
    //   password: widget.model!.password,
    //   accountRole: widget.model!.accountRole,
    //   serviceName: _serviceNameController.text,
    //   location: _locationController.text,
    //   serviceContactPhone: _phoneController.text,
    //   postalCode: _postalController.text,
    //   taxNumber: _taxController.text,
    //   nationalIdNumber: widget.model!.nationalIdNumber,
    //   serviceType: _cubit.state.selectedServiceType,
    //   licenseFile: _cubit.state.licenseFile!,
    // );

    // await _cubit.signup(model, _cubit.state.licenseFile!);
  }

  String? _requiredValidator(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'This field is required';
    }

    return null;
  }

  @override
  Widget build(BuildContext context) {
    return VerificationScaffold(
      model: widget.model,
      stepNumber: 2,
      onSubmit: _submit,
      child: _buildFormCard(),
    );
  }

  Widget _buildFormCard() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(25),
        border: Border.all(color: ColorsManager.cardBorder),
        boxShadow: [
          BoxShadow(
            color: ColorsManager.primaryColor.withOpacity(0.06),
            blurRadius: 20,
            offset: const Offset(0, 5),
          ),
        ],
      ),
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Step 2 of 3',
              style: TextStyle(
                color: ColorsManager.primaryColor,
                fontSize: 14,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),

            Text(
              'Service & Verification Details',
              style: TextStyles.authTitle.copyWith(
                color: ColorsManager.textDark,
              ),
            ),
            const SizedBox(height: 8),

            const Text(
              'Provide your service information and official '
              'business documents.',
              style: TextStyles.fadedText,
            ),
            const SizedBox(height: 25),

            _buildServiceTypeDropdown(),
            const SizedBox(height: 18),

            InputField(
              controller: _serviceNameController,
              labelText: 'Service Name',
              hintText: 'e.g. Al Nour Medical Center',
              isRequired: true,
              validator: _requiredValidator,
            ),
            const SizedBox(height: 18),

            InputField(
              controller: _locationController,
              labelText: 'Location',
              hintText: 'e.g. Cairo',
              isRequired: true,
              validator: _requiredValidator,
            ),
            const SizedBox(height: 18),

            InputField(
              controller: _phoneController,
              labelText: 'Service Contact Phone',
              hintText: '+20 2 1234 5678',
              isRequired: true,
              validator: _requiredValidator,
            ),
            const SizedBox(height: 18),

            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: InputField(
                    controller: _postalController,
                    labelText: 'Postal Code',
                    hintText: 'e.g. 11511',
                    isRequired: true,
                    validator: _requiredValidator,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: InputField(
                    controller: _taxController,
                    labelText: 'Tax Number',
                    hintText: 'e.g. 123-456-789',
                    isRequired: true,
                    validator: _requiredValidator,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 22),

            _buildLicenseUpload(),
          ],
        ),
      ),
    );
  }

  Widget _buildServiceTypeDropdown() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Service Type *',
          style: TextStyle(
            color: ColorsManager.textDark,
            fontSize: 12,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),

        BlocBuilder<ProviderSignupCubit, ProviderSignupState>(
          builder: (context, state) {
            return DropdownButtonFormField<String>(
              value: state.selectedServiceType,
              isExpanded: true,
              dropdownColor: Colors.white,
              decoration: InputDecoration(
                hintText: 'Select service type',
                hintStyle: TextStyles.fadedText,
                filled: true,
                fillColor: Colors.white,
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 14,
                  vertical: 12,
                ),
                border: _dropdownBorder(),
                enabledBorder: _dropdownBorder(),
                focusedBorder: _dropdownBorder(
                  color: ColorsManager.primaryColor,
                  width: 1.5,
                ),
                errorBorder: _dropdownBorder(color: Colors.red),
                focusedErrorBorder: _dropdownBorder(color: Colors.red),
              ),
              icon: const Icon(
                Icons.keyboard_arrow_down_rounded,
                color: ColorsManager.primaryColor,
              ),
              items: ProviderSignupCubit.serviceTypes.map((type) {
                return DropdownMenuItem<String>(
                  value: type,
                  child: Text(
                    type,
                    style: const TextStyle(
                      color: ColorsManager.textDark,
                      fontSize: 14,
                    ),
                  ),
                );
              }).toList(),
              onChanged: state.isSubmitting || state.isPickingFile
                  ? null
                  : _cubit.selectServiceType,
              validator: (value) {
                if (value == null) {
                  return 'Please select a service type';
                }
                return null;
              },
            );
          },
        ),
      ],
    );
  }

  OutlineInputBorder _dropdownBorder({
    Color color = ColorsManager.cardBorder,
    double width = 1,
  }) {
    return OutlineInputBorder(
      borderRadius: BorderRadius.circular(12),
      borderSide: BorderSide(color: color, width: width),
    );
  }

  Widget _buildLicenseUpload() {
    return BlocBuilder<ProviderSignupCubit, ProviderSignupState>(
      builder: (context, state) {
        final file = state.licenseFile;
        final isBusy = state.isPickingFile || state.isSubmitting;

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'License Document *',
              style: TextStyle(
                color: ColorsManager.textDark,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 10),

            InkWell(
              onTap: isBusy ? null : _cubit.pickLicense,
              borderRadius: BorderRadius.circular(16),
              child: Container(
                width: double.infinity,
                padding: const EdgeInsets.symmetric(
                  horizontal: 12,
                  vertical: 20,
                ),
                decoration: BoxDecoration(
                  color: ColorsManager.softBlue,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(
                    color: ColorsManager.primaryColor.withOpacity(0.5),
                    width: 1.5,
                  ),
                ),
                child: Column(
                  children: [
                    if (state.isPickingFile)
                      const SizedBox(
                        height: 38,
                        width: 38,
                        child: CircularProgressIndicator(
                          strokeWidth: 3,
                          color: ColorsManager.primaryColor,
                        ),
                      )
                    else
                      const Icon(
                        Icons.cloud_upload_outlined,
                        color: ColorsManager.primaryColor,
                        size: 38,
                      ),

                    const SizedBox(height: 10),

                    Text(
                      file?.name ?? 'Upload your medical license',
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: ColorsManager.textDark,
                        fontWeight: FontWeight.bold,
                        fontSize: 14,
                      ),
                    ),
                    const SizedBox(height: 5),

                    Text(
                      file == null
                          ? 'PDF, JPG or PNG, up to 10 MB'
                          : '${(state.licenseSize! / (1024 * 1024)).toStringAsFixed(2)} MB',
                      style: TextStyles.fadedText,
                    ),
                    const SizedBox(height: 14),

                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 25,
                        vertical: 10,
                      ),
                      decoration: BoxDecoration(
                        color: ColorsManager.primaryColor,
                        borderRadius: BorderRadius.circular(30),
                      ),
                      child: Text(
                        state.isPickingFile
                            ? 'Selecting...'
                            : file == null
                            ? 'Choose File'
                            : 'Change File',
                        style: TextStyles.buttonText,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
