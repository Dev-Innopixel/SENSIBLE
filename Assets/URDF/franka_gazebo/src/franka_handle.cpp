#include <assert.h>

#include <franka_gazebo/franka_handle.hpp>

namespace gazebo
{

GZ_REGISTER_MODEL_PLUGIN(ForcePlugin)

// Constructor
ForcePlugin::ForcePlugin()
{
    // Set the wrench_msg to {0, 0, 0, 0, 0, 0}, since no wrench should be applied by default.
    this->wrench_msg_.force.x = 0;
    this->wrench_msg_.force.y = 0;
    this->wrench_msg_.force.z = -10.5;
    this->wrench_msg_.torque.x = 0;
    this->wrench_msg_.torque.y = 0;
    this->wrench_msg_.torque.z = 0;
}

// Destructor
ForcePlugin::~ForcePlugin()
{
    // Clear the connection
    update_connection_.reset();

    // Custom callback queue, might not be needed
    this->queue_.clear();
    this->queue_.disable();
    this->rosnode_->shutdown();
    this->callback_queue_thread_.join();
}

// Load the controller
void ForcePlugin::Load( physics::ModelPtr _model, sdf::ElementPtr _sdf )
{
    // Copy world name.
    this->world_ = _model->GetWorld();

    this->robot_namespace_ = "";

    // The namespace utilized

    if(_sdf->HasElement("bodyName"))
        this->robot_namespace_ = _sdf->GetElement("robotNamespace")->Get<std::string>() + "/";

    if(!_sdf->HasElement("updateRate"))
        this->update_rate_ = 1000.;
    else
        this->update_rate_ = _sdf->GetElement("updateRate")->Get<double>();

    // The link name it will send forces too

    if(!_sdf->HasElement("linkName"))
    {
        ROS_FATAL_NAMED("force", "missing <linkName> in plugin, cannot proceed");
        return;
    }
    else
        this->link_name_ = _sdf->GetElement("linkName")->Get<std::string>();
    
    // See if any link is named that

    this->link_ = _model->GetLink(this->link_name_);

    // Check if link was found;

    if(!this->link_)
    {
        ROS_FATAL_NAMED("force", "force plugin error: link named: %s does not exist\n",this->link_name_.c_str());
        auto links = _model->GetLinks();
        for(const auto & link : links)
        {
            ROS_ERROR_STREAM("But name: " << link->GetName() << " was found...");
        }
        return;
    }

    // The topic name it will listen too
    
    if(!_sdf->HasElement("topicName"))
    {
        ROS_FATAL_NAMED("topic", "missing <topicName> in plugin, cannot proceed");
        return;
    }
    else
        this->topic_name_ = _sdf->GetElement("topicName")->Get<std::string>();

    // Make sure a Gazebo rosnode is utilized.
    
    if(!ros::isInitialized())
    {
        ROS_FATAL_STREAM_NAMED("force", "A ROS node for Gazebo has not been initialized, therefore unable to load the plugin franka_handle.so");
        return;
    }

    // Create node
    
    rosnode_ = std::make_unique<ros::NodeHandle>(this->robot_namespace_);

    // Listen to a custom callback queue

    ros::SubscribeOptions so = ros::SubscribeOptions::create<geometry_msgs::Wrench>(
        this->topic_name_,
        1,
        boost::bind(&ForcePlugin::UpdateBodyForce, this, _1),
        ros::VoidPtr(), 
        &this->queue_);
    
    this->sub_ = this->rosnode_->subscribe(so);

    // Create Custom Callback Queue

    this->callback_queue_thread_ = boost::thread(boost::bind(&ForcePlugin::QueueThread, this));

    // Mechanism for updating every world cycle

    this->update_connection_ = event::Events::ConnectWorldUpdateBegin(boost::bind(&ForcePlugin::Update, this));

}

// Update the force applied
void ForcePlugin::UpdateBodyForce(const geometry_msgs::Wrench::ConstPtr & _msg)
{
    wrench_msg_.force.x = _msg->force.x;
    wrench_msg_.force.y = _msg->force.y;
    wrench_msg_.force.z = _msg->force.z;
    wrench_msg_.torque.x = _msg->torque.x;
    wrench_msg_.torque.y = _msg->torque.y;
    wrench_msg_.torque.z = _msg->torque.z;
}

// Update the controller
void ForcePlugin::Update()
{
    this->lock_.lock();
    ignition::math::Vector3d force(this->wrench_msg_.force.x, this->wrench_msg_.force.y, this->wrench_msg_.force.z);
    ignition::math::Vector3d torque(this->wrench_msg_.torque.x, this->wrench_msg_.torque.y, this->wrench_msg_.torque.z);
    this->link_->AddRelativeForce(force);
    this->link_->AddRelativeTorque(torque);
    this->lock_.unlock();
}

// Custom Callback Queue
void ForcePlugin::QueueThread()
{
    while(this->rosnode_->ok())
    {
        this->queue_.callAvailable(ros::WallDuration(1./(this->update_rate_)));
    }
}




}