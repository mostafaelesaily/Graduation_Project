import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class AiNavigatorCard extends StatefulWidget {
  final ValueChanged<String>? onSubmit;

  const AiNavigatorCard({super.key, this.onSubmit});

  @override
  State<AiNavigatorCard> createState() => _AiNavigatorCardState();
}

class _AiNavigatorCardState extends State<AiNavigatorCard> {
  static const List<String> _quickActions = [
    '👶  Pediatrician near me',
    '🩸  Book Blood Test',
  ];

  final TextEditingController _controller = TextEditingController();
  bool _aiEnabled = true;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _submit() {
    final text = _controller.text.trim();
    if (text.isEmpty) return;
    widget.onSubmit?.call(text);
    FocusScope.of(context).unfocus();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.all(16.w),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.7),
        borderRadius: BorderRadius.circular(24.r),
        border: Border.all(color: ColorsManager.cardBorder),
        boxShadow: [
          BoxShadow(
            color: ColorsManager.primaryColor.withValues(alpha: 0.06),
            blurRadius: 24,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: EdgeInsets.all(8.w),
                decoration: BoxDecoration(
                  color: ColorsManager.softBlue,
                  borderRadius: BorderRadius.circular(10.r),
                ),
                child: Icon(
                  Icons.auto_awesome,
                  color: ColorsManager.primaryColor,
                  size: 18.sp,
                ),
              ),
              SizedBox(width: 10.w),
              Expanded(
                child: Text(
                  'MedLink AI Navigator',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: ColorsManager.textDark,
                    fontSize: 16.sp,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              SizedBox(width: 8.w),
              _ToggleButton(
                enabled: _aiEnabled,
                onTap: () => setState(() => _aiEnabled = !_aiEnabled),
              ),
            ],
          ),
          SizedBox(height: 14.h),
          Container(
            padding: EdgeInsets.fromLTRB(12.w, 8.h, 8.w, 8.h),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16.r),
              border: Border.all(color: ColorsManager.cardBorder),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.search,
                  color: ColorsManager.primaryColor,
                  size: 22.sp,
                ),
                SizedBox(width: 8.w),
                Expanded(
                  child: TextField(
                    controller: _controller,
                    onSubmitted: (_) => _submit(),
                    textInputAction: TextInputAction.search,
                    style: TextStyle(fontSize: 14.sp),
                    decoration: InputDecoration(
                      isDense: true,
                      border: InputBorder.none,
                      hintText: _aiEnabled
                          ? 'Describe symptoms, specialty...'
                          : 'Search doctors, clinics...',
                      hintStyle: TextStyle(
                        color: ColorsManager.fadedText,
                        fontSize: 14.sp,
                      ),
                    ),
                  ),
                ),
                SizedBox(width: 8.w),
                Material(
                  color: ColorsManager.primaryColor,
                  borderRadius: BorderRadius.circular(12.r),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(12.r),
                    onTap: _submit,
                    child: SizedBox(
                      width: 42.w,
                      height: 42.w,
                      child: Icon(
                        Icons.send_rounded,
                        color: Colors.white,
                        size: 20.sp,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
          SizedBox(height: 12.h),
          Wrap(
            spacing: 10.w,
            runSpacing: 8.h,
            children: [
              for (final label in _quickActions)
                ActionChip(
                  label: Text(label),
                  labelStyle: TextStyle(
                    color: ColorsManager.primaryColor,
                    fontSize: 13.sp,
                    fontWeight: FontWeight.w600,
                  ),
                  backgroundColor: ColorsManager.softBlue,
                  side: BorderSide(color: ColorsManager.cardBorder),
                  shape: const StadiumBorder(),
                  onPressed: () {
                    _controller.text = label.substring(label.indexOf(' ') + 1).trim();
                    _controller.selection = TextSelection.collapsed(
                      offset: _controller.text.length,
                    );
                  },
                ),
            ],
          ),
        ],
      ),
    );
  }
}

class _ToggleButton extends StatelessWidget {
  final bool enabled;
  final VoidCallback onTap;

  const _ToggleButton({required this.enabled, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: enabled ? ColorsManager.primaryColor : ColorsManager.softBlue,
      borderRadius: BorderRadius.circular(12.r),
      child: InkWell(
        borderRadius: BorderRadius.circular(12.r),
        onTap: onTap,
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 14.w, vertical: 10.h),
          child: Text(
            'Toggle AI',
            style: TextStyle(
              color: enabled ? Colors.white : ColorsManager.primaryColor,
              fontSize: 13.sp,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
      ),
    );
  }
}
